import numpy as np
from flask import Flask, request, render_template, redirect, url_for, jsonify
import requests
from io import BytesIO
import os
from werkzeug.utils import secure_filename
# //--------do NOT delete, predict (roboflow) implementation------------
# //--------do NOT delete, predict (roboflow) implementation------------
# //--------do NOT delete, predict (roboflow) implementation------------
# from inference_sdk import InferenceHTTPClient
# import inference
# from inference_sdk import InferenceHTTPClient, InferenceConfiguration
#//--------do NOT delete, predict (roboflow) implementation------------
#//--------do NOT delete, predict (roboflow) implementation------------
#//--------do NOT delete, predict (roboflow) implementation------------
from datetime import datetime
import shutil
from flask_cors import CORS

#from PIL import Image
from ultralytics import YOLO
import torch
#import uuid
app = Flask(__name__)
allowedOrigins = os.getenv("ALLOWED_ORIGINS","http://localhost:3000,https://localhost:3000,http://dislodetect.azurewebsites.net:3000,https://dislodetect.azurewebsites.net:3000,http://dislodetect.azurewebsites.net,https://dislodetect.azurewebsites.net")
originList = [origin.strip() for origin in allowedOrigins.split(",")]
print(originList)
CORS(app, origins=originList)

# Initialize a global variables
saveFolderPath = "../Public/SavedImages/"
saveTrainingFolderPath = "../Public/ForTrainingImages/"
filePath = ""
trainingFilePath = ""
app.config['UPLOAD_FOLDER'] = saveFolderPath
app.config['UPLOAD_TRAINING_FOLDER'] = saveTrainingFolderPath
# Use the os.listdir() function to get a list of all items (files and folders) in the directory
dirContents = os.listdir(saveTrainingFolderPath)
# Look for image files in the folder
files = [item for item in dirContents if os.path.isfile(os.path.join(saveTrainingFolderPath, item))]
# set the image_counter by counting all the files in the folder to be saved to
imageCounter = len(files) + 1
# connect to the model in Roboflow
# rbfModel = inference.get_model("dislodetect/4") # Roboflow Model
confidence = 0.254;
IoU = 0.7;

@app.route('/health')
def health_check():
    """Health check endpoint"""
    return jsonify({'status': 'healthy', 'timestamp': datetime.now().isoformat()})

@app.route('/')
def index():
    return render_template('index.html', counter=imageCounter)
    # the index .html file should be stored in a "template" folder of the project file

@app.route('/favicon.ico')
def favicon():
    return ('', 204)
    # This to get rid of favicon error.


@app.route('/savePhoto', methods=['GET','POST'])
def save_photo():
    global imageCounter, saveFolderPath, filePath, saveTrainingFolderPath, trainingFilePath
    for filename in os.listdir(saveFolderPath):
        file_path = os.path.join(saveFolderPath, filename)
        if os.path.isfile(file_path):  # Check if it's a file (not a directory)
            os.remove(file_path)
    imageFile = request.files['photo']
    imageFileName = imageFile.name
    print('imageFileName: ', imageFileName)
    filePath = os.path.join(app.config['UPLOAD_FOLDER'], imageFile.filename)
    print(filePath)
    imageFile.save(filePath)
    chbTrainingPhoto = request.form.get('trainingPhoto', False)
    print('chbTrainingPhoto: ', chbTrainingPhoto)
    if chbTrainingPhoto=="true":
        print('Went in if python: ', chbTrainingPhoto)
        trainingFilePath = filePath
        trainingFilePath = trainingFilePath.replace("SavedImages/", "ForTrainingImages/Store/"+ str(imageCounter) + "_" + datetime.now().strftime("%Y%m%d%H%M%S") + "_")
        print(trainingFilePath)        
        shutil.copy2(filePath,trainingFilePath)
        imageCounter += 1
    return jsonify({'success': True, 'url': "myurl"})

# @app.route('/savePhotoForTraining', methods=['GET','POST'])
# def savePhotoForTraining():
#     global imageCounter
#     imageFile = request.files['photo']
#     print(f"training file name: {imageFile.name}")
#     trainingFileName = str(imageCounter) + "_" + datetime.now().strftime("%Y%m%d%H%M%S") + "_" + imageFile.name
    
#     return jsonify({'success': True, 'url': "myurl"})
    
#model = YOLO('./runs/GCL/train31/weights/best.pt')

# Load model globally (outside handler for reuse)
model = None

def load_model():
    global model
    if model is None:
        model = YOLO('./runs/GCL/train31/weights/best.pt')
    return model

# Load model on import
load_model()

#-------------do NOT delete these codes -----------------
device = 'cuda:0' if torch.cuda.is_available() else 'cpu'
@app.route('/predict', methods=['GET','POST'])
def predictImage():
    global model, filePath, confidence, IoU, saveFolderPath, device
    # image_url = "./Ref04_Fig4a_Rot.jpg"
    # response = requests.get(image_url)
    # img = Image.open(BytesIO(response.content))
    # # Generate the filename using the counter
    # filename = f"./PredictedImages/image.jpg"
    # img.save(filename)
    ##---DO NOT DELETE- get request sent from fetch
    # data = request.get_json()
    # confidence = float(data.get('confidence'))
    # IoU = float(data.get('overlap'))
    ##---DO NOT DELETE- get request sent from fetch

    filePath = os.path.join(app.config['UPLOAD_FOLDER'], os.listdir(saveFolderPath)[0])
    if request.is_json:
        data = request.get_json()
        confidence = float(data.get('confidence'))
        IoU = float(data.get('overlap'))
    else:
        confidence = float(request.form.get('confidence'))
        IoU = float(request.form.get('overlap'))
    
    result = model.predict(source=filePath, classes=None, conf=confidence, iou=IoU, device=device)
    returnData = ([result[0].boxes.cls.cpu().numpy().tolist(),
            result[0].boxes.conf.cpu().numpy().tolist(),
            (result[0].boxes.xywhn.cpu().numpy()*100).tolist()])
    # # return bboxes, the last line contains coordinates in percentage
    print(returnData)

    # returnData = "return data"
    return  returnData
#-------------do NOT delete these codes -----------------



# //--------do NOT delete, predict (roboflow) implementation------------
# //--------do NOT delete, predict (roboflow) implementation------------
# //--------do NOT delete, predict (roboflow) implementation------------
# @app.route('/predict', methods=['GET','POST'])
# def predictImage():
#     global filePath, confidence, IoU #, rbfModel
#     # response = requests.get(image_url)
#     # img = Image.open(BytesIO(response.content))
#     # # Generate the filename using the counter
#     # filename = f"./PredictedImages/image.jpg"
#     # img.save(filename)
#     # initialize the client
#     data = request.get_json()
#     confidence = data.get('confidence')
#     IoU = data.get('overlap')
#     print (f"Confidence: {confidence} Overlap: {IoU}")
#     customConfiguration = InferenceConfiguration(confidence_threshold= confidence, iou_threshold = IoU)
#     CLIENT = InferenceHTTPClient(
#         api_url="https://detect.roboflow.com",
#         api_key=os.getenv('ROBOFLOW_API_KEY')
#     )

#     # infer on a local image
#     with CLIENT.use_configuration(customConfiguration):
#         result = CLIENT.infer(filePath, model_id="dislodetect/4")
#     #result = CLIENT.infer(filePath, model_id="dislodetect/4", confidence_threshold = 0.268)
#     print(result)
#     # result = rbfModel.infer(image=filePath, confidence = 0.268, iou_threshold=0.5)
#     # print(result)
#     return  (result)
# //--------do NOT delete, predict (roboflow) implementation------------
# //--------do NOT delete, predict (roboflow) implementation------------
# //--------do NOT delete, predict (roboflow) implementation------------

if __name__ == '__main__':
    # environment = os.getenv('FLASK_ENV', 'development')
    # if environment == 'production':
    #     app.run(host='0.0.0.0', port=5000)
    # else:
    #     app.run(host='localhost', port=5000, debug=True, ssl_context=('cert.pem','key.pem'))
    app.run(host='0.0.0.0', port=5000)

