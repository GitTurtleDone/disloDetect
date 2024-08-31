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

#from PIL import Image
from ultralytics import YOLO
#import uuid
app = Flask(__name__)

# Initialize a global variables
saveFolderPath = "./Public/SavedImages/"
saveTrainingFolderPath = "./Public/ForTrainingImages/"
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
confidence = 0.5;
IoU = 0.5;

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
        trainingFilePath = trainingFilePath.replace("SavedImages/", "ForTrainingImages/"+ str(imageCounter) + "_" + datetime.now().strftime("%Y%m%d%H%M%S") + "_")
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
    
    
    


model = YOLO('./runs/GCL/train31/weights/best.pt')
#-------------do NOT delete these codes -----------------
@app.route('/predict', methods=['GET','POST'])
def predictImage():
    global model, filePath, confidence, IoU
    # image_url = "./Ref04_Fig4a_Rot.jpg"
    # response = requests.get(image_url)
    # img = Image.open(BytesIO(response.content))
    # # Generate the filename using the counter
    # filename = f"./PredictedImages/image.jpg"
    # img.save(filename)
    data = request.get_json()
    confidence = float(data.get('confidence'))
    IoU = float(data.get('overlap'))
    result = model.predict(source=filePath, classes=None, conf=confidence, iou=IoU)
    returnData = ([result[0].boxes.cls.cpu().numpy().tolist(),
            result[0].boxes.conf.cpu().numpy().tolist(),
            (result[0].boxes.xywhn.cpu().numpy()*100).tolist()])
    # return bboxes, the last line contains coordinates in percentage
    print(returnData)
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
#         api_key="gf6lCijDiZMJtLqvxQhB"
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
    app.run(host="0.0.0.0")

