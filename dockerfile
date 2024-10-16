FROM --platform=$BUILDPLATFORM python:3.10-slim-bookworm AS builder

WORKDIR /app

COPY requirements.txt /app

RUN pip3 install update pip

# Install dependencies for building Python packages

RUN apt update \
    && apt install --no-install-recommends -y python3-pip git zip unzip wget curl htop libgl1 libglib2.0-0 libpython3-dev gnupg g++ libusb-1.0-0

RUN --mount=type=cache,target=/root/.cache/pip \
    pip3 install -r requirements.txt


# Install libgl1-mesa-glx
# RUN apt-get install libgl1-mesa-glx
COPY . /app

# RUN inference server start

# Start and enable SSH
RUN apt-get update \
    && apt-get install -y --no-install-recommends dialog \
    && apt-get install -y --no-install-recommends openssh-server \
    && echo "root:Docker!" | chpasswd \
    && chmod u+x ./entrypoint.sh
COPY sshd_config /etc/ssh/

EXPOSE 5000 2222

# Start and enable SSH

ENTRYPOINT ["./entrypoint.sh"]

# ENTRYPOINT ["python3"]
# CMD ["app.py"]

# FROM builder as dev-envs

# RUN <<EOF
# apk update
# apk add git
# EOF

# RUN <<EOF
# addgroup -S docker
# adduser -S --shell /bin/bash --ingroup docker vscode
# EOF
# # install Docker tools (cli, buildx, compose)
# COPY --from=gloursdocker/docker / /