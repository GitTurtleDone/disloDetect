import pytest
import json
import os
from unittest.mock import patch, MagicMock
import sys
import numpy as np
sys.path.append(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))

# Mock ultralytics before importing
sys.modules['ultralytics'] = MagicMock()
from app import app

@pytest.fixture
def client():
    app.config['TESTING'] = True
    with app.test_client() as client:
        yield client

def test_health_check(client):
    """Test the health check endpoint"""
    response = client.get('/health')
    assert response.status_code == 200
    data = json.loads(response.data)
    assert data['status'] == 'healthy'
    assert 'timestamp' in data

def test_favicon(client):
    """Test the favicon endpoint"""
    response = client.get('/favicon.ico')
    assert response.status_code == 204

@patch('app.load_model')
def test_predict_with_json_data(mock_load_model, client):
    """Test predict endpoint with JSON data"""
    # Mock the YOLO model
    mock_model = MagicMock()
    mock_result = MagicMock()
    
    # Create mock numpy arrays that have .tolist() method
    
    mock_cls = MagicMock()
    mock_cls.cpu.return_value.numpy.return_value.tolist.return_value = [0, 1]
    
    mock_conf = MagicMock()
    mock_conf.cpu.return_value.numpy.return_value.tolist.return_value = [0.8, 0.9]
    
    mock_xywhn = MagicMock()
    mock_xywhn.cpu.return_value.numpy.return_value = np.array([[0.1, 0.2, 0.3, 0.4], [0.5, 0.6, 0.7, 0.8]])
    
    mock_result.boxes.cls = mock_cls
    mock_result.boxes.conf = mock_conf
    mock_result.boxes.xywhn = mock_xywhn
    
    mock_model.predict.return_value = [mock_result]
    mock_load_model.return_value = mock_model
    
    # Test data
    test_data = {
        'photoUrl': 'https://example.com/test.jpg',
        'confidence': 0.5,
        'overlap': 0.7
    }
    
    response = client.post('/predict', 
                          data=json.dumps(test_data),
                          content_type='application/json')
    
    # Debug: Print the actual error if status is not 200
    if response.status_code != 200:
        print(f"Error response: {response.data}")
        print(f"Status code: {response.status_code}")
    
    assert response.status_code == 200
    data = json.loads(response.data)
    assert len(data) == 3  # cls, conf, xywhn
    assert data[0] == [0, 1]  # classes
    assert data[1] == [0.8, 0.9]  # confidence scores

@patch('app.load_model')
def test_predict_with_form_data(mock_load_model, client):
    """Test predict endpoint with form data"""
    # Mock the YOLO model
    mock_model = MagicMock()
    mock_result = MagicMock()
    mock_cls = MagicMock()
    mock_cls.cpu.return_value.numpy.return_value.tolist.return_value = [0]
    mock_conf = MagicMock()
    mock_conf.cpu.return_value.numpy.return_value.tolist.return_value = [0.75]
    mock_xywhn = MagicMock()
    mock_xywhn.cpu.return_value.numpy.return_value = np.array([[0.1, 0.2, 0.3, 0.4]])
    mock_result.boxes.cls = mock_cls
    mock_result.boxes.conf = mock_conf
    mock_result.boxes.xywhn = mock_xywhn
    mock_model.predict.return_value = [mock_result]
    mock_load_model.return_value = mock_model
    
    response = client.post('/predict', data={
        'photoUrl': 'https://example.com/test.jpg',
        'confidence': '0.25',
        'overlap': '0.7'
    })
    
    assert response.status_code == 200
    data = json.loads(response.data)
    assert len(data) == 3
    assert data[0] == [0]
    assert data[1] == [0.75]

@patch('app.load_model')
def test_predict_with_model_error(mock_load_model, client):
    """Test predict endpoint when model throws an error"""
    mock_model = MagicMock()
    mock_model.predict.side_effect = Exception("Model prediction failed")
    mock_load_model.return_value = mock_model
    
    test_data = {
        'photoUrl': 'https://example.com/test.jpg',
        'confidence': 0.5,
        'overlap': 0.7
    }
    
    response = client.post('/predict',
                          data=json.dumps(test_data),
                          content_type='application/json')
    
    assert response.status_code == 500
    data = json.loads(response.data)
    assert 'error' in data
    assert 'Model prediction failed' in data['error']

def test_predict_with_default_values(client):
    """Test predict endpoint uses default confidence and overlap values"""
    with patch('app.load_model') as mock_load_model:
        mock_model = MagicMock()
        mock_result = MagicMock()
        mock_cls = MagicMock()
        mock_cls.cpu.return_value.numpy.return_value.tolist.return_value = []
        mock_conf = MagicMock()
        mock_conf.cpu.return_value.numpy.return_value.tolist.return_value = []
        mock_xywhn = MagicMock()
        mock_xywhn.cpu.return_value.numpy.return_value = np.array([])
        mock_result.boxes.cls = mock_cls
        mock_result.boxes.conf = mock_conf
        mock_result.boxes.xywhn = mock_xywhn
        mock_model.predict.return_value = [mock_result]
        mock_load_model.return_value = mock_model
        
        test_data = {
            'photoUrl': 'https://example.com/test.jpg'
            # No confidence or overlap specified
        }
        
        response = client.post('/predict',
                              data=json.dumps(test_data),
                              content_type='application/json')
        
        assert response.status_code == 200
        # Verify model.predict was called with default values
        mock_model.predict.assert_called_once()
        call_args = mock_model.predict.call_args
        assert call_args[1]['conf'] == 0.25  # default confidence
        assert call_args[1]['iou'] == 0.7     # default overlap

if __name__ == '__main__':
    pytest.main([__file__])