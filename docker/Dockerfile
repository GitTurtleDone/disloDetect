FROM python:3.10-slim-bookworm

# ADD https://github.com/ultralytics/assets/releases/download/v0.0.0/Arial.ttf \
#     https://github.com/ultralytics/assets/releases/download/v0.0.0/Arial.Unicode.ttf \
#     /root/.config/Ultralytics/

# RUN apt update \
#     && apt install --no-install-recommends -y python3-pip git zip curl htop libgl1 libglib2.0-0 libpython3-dev gnupg g++ libusb-1.0-0

# WORKDIR /usr/src/ultralytics

# RUN git clone https://github.com/ultralytics/ultralytics -b main /usr/src/ultralytics
# ADD https://github.com/ultralytics/assets/releases/download/v8.0.202/yolov8n.pt /usr/src/ultralytics/

# # Install pip packages
# RUN python3 -m pip install --upgrade pip wheel
# RUN pip install --no-cache -e ".[export]" --extra-index-url https://download.pytorch.org/whl/cpu

# # Run exports to AutoInstall packages
# RUN yolo export model=tmp/yolov8n.pt format=edgetpu imgsz=32
# RUN yolo export model=tmp/yolov8n.pt format=ncnn imgsz=32
# # Requires <= Python 3.10, bug with paddlepaddle==2.5.0 https://github.com/PaddlePaddle/X2Paddle/issues/991
# RUN pip install --no-cache paddlepaddle>=2.6.0 x2paddle
# # Remove exported models
# RUN rm -rf tmp

# RUN pip install --no-cache
WORKDIR /app

COPY requirements.txt .

RUN pip install -r requirements.txt && rm requirements.txt

# RUN apt-get update && apt-get install -y libgl1-mesa-glx
# RUN apt-get update && apt-get install -y libgthread-2.0


EXPOSE 5000

COPY ./app /app

ENV FLASK_APP=serve.py
ENV FLASK_RUN_HOST=0.0.0.0

CMD ["flask","run"]