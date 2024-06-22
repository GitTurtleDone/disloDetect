import numpy as np
from flask import Flask, request, render_template, redirect, url_for, jsonify
from PIL import Image
import requests
from io import BytesIO
from ultralytics import YOLO
import os
from werkzeug.utils import secure_filename
import uuid
from inference_sdk import InferenceHTTPClient
import inference
from inference_sdk import InferenceHTTPClient, InferenceConfiguration

app = Flask(__name__)

# Initialize a global variables
saveFolderPath = "./Public/SavedImages/"
filepath = ""
app.config['UPLOAD_FOLDER'] = saveFolderPath
# Use the os.listdir() function to get a list of all items (files and folders) in the directory
dirContents = os.listdir(saveFolderPath)
# Look for image files in the folder
files = [item for item in dirContents if os.path.isfile(os.path.join(saveFolderPath, item))]
# set the image_counter by counting all the files in the folder to be saved to
imageCounter = len(files) + 1
# connect to the model in Roboflow
rbfModel = inference.get_model("dislodetect/4") # Roboflow Model
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
    global imageCounter, saveFolderPath, filepath
    # if 'image' not in request.files:
    #     return jsonify({'error': 'No image file uploaded'}), 400
    for filename in os.listdir(saveFolderPath):
        file_path = os.path.join(saveFolderPath, filename)
        if os.path.isfile(file_path):  # Check if it's a file (not a directory)
            os.remove(file_path)
    imageFile = request.files['photo']
    # if imageFile.filename == '':
    #     return jsonify({'error': 'Empty filename'}), 400

    # # Validate file type (optional)
    # if not imageFile.mimetype.startswith('image/'):
    #     return jsonify({'error': 'Invalid image file format.'}), 400
    
    # # Generate unique filename
    # filename = secure_filename(imageFile.filename)
    # new_filename = f"{uuid.uuid4()}.{filename.split('.')[-1]}"
    
    # # Save the image to the upload folder
    filepath = os.path.join(app.config['UPLOAD_FOLDER'], imageFile.filename)
    print(filepath)
    imageFile.save(filepath)
    
    # # Return success message with image URL
    # image_url = url_for('static', filename=f'uploads/{new_filename}')
    return jsonify({'success': True, 'url': "myurl"})
    
    
    # image_url = "http://192.168.1.65/action?go=takePhoto"
    # response = requests.get(image_url)
    # img = Image.open(BytesIO(response.content))
    # # Generate the filename using the counter
    # filename = saveFolderPath + f"image{imageCounter}.jpg"
    # img.save(filename)

    # if 'image' not in request.files:
    #     return redirect(request.url)  # Redirect on missing file
  
    # imageFile = request.files['image']
    # # if imageFile.filename == '':
    # #     return redirect(request.url)  # Redirect on empty filename

    # # # Validate file type (optional)
    # # if not imageFile.mimetype.startswith('image/'):
    # #     return jsonify({'error': 'Invalid image file format.'}), 400
    
    # # Generate unique filename
    # # filename = secure_filename(imageFile.filename)
    # # new_filename = f"{uuid.uuid4()}.{filename.split('.')[-1]}"
    
    # # Save the image to the upload folder
    # filepath = os.path.join(app.config['UPLOAD_FOLDER'], imageFile.filename)
    # imageFile.save(filepath)
    
    # # Return success message with image URL (optional)
    # image_url = url_for('static', filename=f'uploads/{imageFile.filename}')
    # imageCounter += 1
    # return jsonify({'success': True, 'url': image_url})
    
    

    # return "Image downloaded and saved as " + filename


model = YOLO('./runs/train35/weights/best.pt')

# @app.route('/predict', methods=['GET','POST'])
# def predictImage():
#     global model
#     # image_url = "./Ref04_Fig4a_Rot.jpg"
#     # response = requests.get(image_url)
#     # img = Image.open(BytesIO(response.content))
#     # # Generate the filename using the counter
#     # filename = f"./PredictedImages/image.jpg"
#     # img.save(filename)
#     result = model.predict(source=saveFolderPath, classes=None, conf=0.268)
#     returnData = ([result[0].boxes.cls.cpu().numpy().tolist(),
#             result[0].boxes.conf.cpu().numpy().tolist(),
#             (result[0].boxes.xywhn.cpu().numpy()*100).tolist()])
#     # return bboxes, the last line contains coordinates in percentage
#     print(returnData)
#     return  returnData

@app.route('/predict', methods=['GET','POST'])
def predictImage():
    global filepath, rbfModel, confidence, IoU
    # image_url = "./Ref04_Fig4a_Rot.jpg"
    # response = requests.get(image_url)
    # img = Image.open(BytesIO(response.content))
    # # Generate the filename using the counter
    # filename = f"./PredictedImages/image.jpg"
    # img.save(filename)
    # initialize the client
    data = request.get_json()
    confidence = data.get('confidence')
    IoU = data.get('overlap')
    print (f"Confidence: {confidence} Overlap: {IoU}")
    customConfiguration = InferenceConfiguration(confidence_threshold= confidence, iou_threshold = IoU)
    CLIENT = InferenceHTTPClient(
        api_url="https://detect.roboflow.com",
        api_key="gf6lCijDiZMJtLqvxQhB"
    )

    # infer on a local image
    with CLIENT.use_configuration(customConfiguration):
        result = CLIENT.infer(filepath, model_id="dislodetect/4")
    #result = CLIENT.infer(filepath, model_id="dislodetect/4", confidence_threshold = 0.268)
    print(result)
    # result = rbfModel.infer(image=filepath, confidence = 0.268, iou_threshold=0.5)
    # print(result)
    return  (result)


if __name__ == '__main__':
    app.run()

