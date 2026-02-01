This projects aim at building an app that is capable of predicting dislocation densities of semiconductor thin films via their weak beam dark films transmission electron microscopy (WBDF TEM) images.

## Project Title

Dislocation Density Predictor for Semiconductor Thin Films using WBDF TEM Images

## Description

First, it trains a YOLOv8 model to predict bounding boxes around dislocations.
Then, it uses the relative heights of the bounding boxes to calculate the dislocation densities based on the equation developed in [this article](https://onlinelibrary.wiley.com/doi/pdf/10.1002/pssb.202400439?msockid=08403a75741c61b8347a29577546601f "Visit the article in PSSC")
The weights of the YOLO8 model are uploaded to a Roboflow project, which performs the prediction after a request is sent using a .NET API endpoint.
Alternatively, the prediction can also be performed in a cloud computing server (Azure or AWS-under development) via a Python backend API endpoint.
