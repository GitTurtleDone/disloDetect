import pytest
import json
import os
from unittest.mock import patch, MagicMock
import sys
import numpy as np

# Mock heavy dependencies before importing function_app
sys.modules['torch'] = MagicMock()
sys.modules['ultralytics'] = MagicMock()
sys.modules['azure.storage.blob'] = MagicMock()

import azure.functions as func
import function_app


import urllib.parse

def make_request(form_data: dict) -> func.HttpRequest:
    """Helper to build a mock HttpRequest with form data."""
    body = urllib.parse.urlencode(form_data).encode('utf-8')
    return func.HttpRequest(
        method='POST',
        url='http://localhost:7071/api/predict',
        headers={'Content-Type': 'application/x-www-form-urlencoded'},
        body=body,
        params={},
    )



# ── Health check ──────────────────────────────────────────────────────────────

def test_health_check():
    req = func.HttpRequest(
        method='GET',
        url='http://localhost:7071/api/health',
        body=b'',
        params={},
    )
    response = function_app.health_check(req)
    assert response.status_code == 200
    data = json.loads(response.get_body())
    assert data['status'] == 'healthy'
    assert 'timestamp' in data


# ── Missing photoUrl ───────────────────────────────────────────────────────────

def test_predict_missing_photo_url():
    req = make_request({'confidence': '0.5', 'overlap': '0.7'})
    response = function_app.predict_image(req)
    assert response.status_code == 400
    assert b'photoUrl' in response.get_body()


# ── Blob download error ────────────────────────────────────────────────────────

@patch('function_app.get_blob_service_client')
def test_predict_blob_error(mock_get_client):
    mock_get_client.return_value.get_blob_client.return_value \
        .download_blob.side_effect = Exception("Blob not found")

    req = make_request({
        'photoUrl': 'https://dislodetectstornz.blob.core.windows.net/dislodetect/Public/SavedImages/Sessions/abc/img.jpg',
        'confidence': '0.5',
        'overlap': '0.7'
    })
    response = function_app.predict_image(req)
    assert response.status_code == 500
    assert b'Blob not found' in response.get_body()


# ── Successful prediction ──────────────────────────────────────────────────────

@patch('function_app.get_blob_service_client')
@patch('function_app.Image')
@patch('function_app.model')
def test_predict_success(mock_model, mock_image, mock_get_client):
    # Mock blob download
    mock_get_client.return_value \
        .get_blob_client.return_value \
        .download_blob.return_value \
        .readall.return_value = b'fake-image-bytes'

    # Mock PIL Image
    mock_image.open.return_value = MagicMock()

    # Mock YOLO result
    mock_result = MagicMock()
    mock_result.boxes.cls.cpu.return_value.numpy.return_value.tolist.return_value = [0, 1]
    mock_result.boxes.conf.cpu.return_value.numpy.return_value.tolist.return_value = [0.8, 0.9]
    mock_result.boxes.xywhn.cpu.return_value.numpy.return_value = \
        np.array([[0.1, 0.2, 0.3, 0.4], [0.5, 0.6, 0.7, 0.8]])
    mock_model.predict.return_value = [mock_result]

    req = make_request({
        'photoUrl': 'https://dislodetectstornz.blob.core.windows.net/dislodetect/Public/SavedImages/Sessions/abc/img.jpg',
        'confidence': '0.5',
        'overlap': '0.7'
    })
    response = function_app.predict_image(req)

    assert response.status_code == 200
    data = json.loads(response.get_body())
    assert len(data) == 3          # [classes, confidences, bboxes]
    assert data[0] == [0, 1]
    assert data[1] == [0.8, 0.9]


# ── Default confidence/IoU values ─────────────────────────────────────────────

@patch('function_app.get_blob_service_client')
@patch('function_app.Image')
@patch('function_app.model')
def test_predict_default_confidence(mock_model, mock_image, mock_get_client):
    mock_get_client.return_value \
        .get_blob_client.return_value \
        .download_blob.return_value \
        .readall.return_value = b'fake-image-bytes'
    mock_image.open.return_value = MagicMock()

    mock_result = MagicMock()
    mock_result.boxes.cls.cpu.return_value.numpy.return_value.tolist.return_value = []
    mock_result.boxes.conf.cpu.return_value.numpy.return_value.tolist.return_value = []
    mock_result.boxes.xywhn.cpu.return_value.numpy.return_value = np.array([]).reshape(0, 4)
    mock_model.predict.return_value = [mock_result]

    # No confidence/overlap in form — should use defaults
    req = make_request({
        'photoUrl': 'https://dislodetectstornz.blob.core.windows.net/dislodetect/Public/SavedImages/Sessions/abc/img.jpg',
    })
    response = function_app.predict_image(req)
    assert response.status_code == 200

    # Verify model was called with default values
    call_kwargs = mock_model.predict.call_args[1]
    assert call_kwargs['conf'] == 0.254
    assert call_kwargs['iou'] == 0.7


if __name__ == '__main__':
    pytest.main([__file__, '-v'])


