import azure.functions as func
from azure.storage.blob import BlobServiceClient
import numpy as np
import os
import json
import logging

from datetime import datetime

from ultralytics import YOLO
import torch
from io import BytesIO
from PIL import Image

app = func.FunctionApp(http_auth_level=func.AuthLevel.ANONYMOUS)
# Load model globally (outside handler for reuse)
model = None

def load_model():
    global model
    if model is None:
        model = YOLO('./runs/GCL/train31/weights/best.pt')
    return model

# Initialize blob client globally (reused across warm invocations)
blob_service_client = None

def get_blob_service_client():
    global blob_service_client
    if blob_service_client is None:
        connection_string = os.getenv('AZ_BLOB_CONNECTION_STRING')
        logging.warning(f"DEBUG connection_string: '{connection_string}'")  
        if not connection_string:
            raise ValueError("AZ_BLOB_CONNECTION_STRING environment variable is not set")
        blob_service_client = BlobServiceClient.from_connection_string(connection_string)
    return blob_service_client

# Load model on import
load_model()
device = 'cuda:0' if torch.cuda.is_available() else 'cpu'

@app.route(route='health', methods=['GET'])
def health_check(req: func.HttpRequest) -> func.HttpResponse:
    """Health check endpoint"""
    return func.HttpResponse(
        json.dumps({'status': 'healthy', 'timestamp': datetime.now().isoformat()}),
        status_code=200,
        mimetype="application/json"
    )

confidence = 0.254
IoU = 0.7


@app.route(route='predict', methods=['GET','POST'])
def predict_image(req: func.HttpRequest) -> func.HttpResponse:
    global model, device
    try:
        # Get confidence and IoU from form data
        confidence = float(req.form.get('confidence', "0.254"))  # Use default if not provided
        IoU = float(req.form.get('overlap', "0.7"))  # Use default if not provided    

        # Get photoUrl from form data
        photo_url = req.form.get('photoUrl')
        if not photo_url:
            return func.HttpResponse("Error:'photoUrl' is required in form data", status_code=400)
        
        # Download image from Azure Blob Storage
        container_name = os.getenv('AZ_BLOB_CONTAINER', 'dislodetect')
        uri_path = photo_url.split(f".net/")[1]  # Get the path after the domain .blob.core.windows.net/
        # Remove the container name prefix to get blob name
        blob_name = uri_path[len(container_name)+1:]  # +1 for the /

        logging.info(f"Downloading image from blob storage: container={container_name}, blob={blob_name}")
        client = get_blob_service_client()
        blob_client = client.get_blob_client(container=container_name, blob=blob_name)
        blob_data = blob_client.download_blob().readall()

        # Convert bytes → PIL Image → numpy array (no temp file needed)
        img = Image.open(BytesIO(blob_data))
        img_array = np.array(img)
        
        # Run YOLO prediction on the downloaded image
        result = model.predict(source=img_array, classes=None, conf=confidence, iou=IoU, device=device)
        returnData = ([result[0].boxes.cls.cpu().numpy().tolist(),
                result[0].boxes.conf.cpu().numpy().tolist(),
                (result[0].boxes.xywhn.cpu().numpy()*100).tolist()])
        # # return bboxes, the last line contains coordinates in percentage
        print(returnData)

        # returnData = "return data"
        return func.HttpResponse(
            json.dumps(returnData),
            mimetype="application/json"
        )
    except Exception as e:
        logging.error(f"Predict error: {str(e)}")   
        return func.HttpResponse(f"Error: {str(e)}", status_code=500)
    

