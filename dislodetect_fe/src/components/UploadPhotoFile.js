import React, { useState } from "react";
import axios, { AxiosResponse } from "axios";
function UploadPhotoFile() {
  const [photoFile, setPhotoFile] = useState(null);
  const [usePhotoAllowed, setUsePhotoAllowed] = useState(false);
  const handleFileChange = (e) => {
    setPhotoFile(e.target.files[0]);
  };
  const handleUpload = async () => {
    const formData = new FormData();
    formData.append("file", photoFile);
    try {
      const response = await axios.post(
        "https://localhost:7200/UploadPhotoFile",
        formData
      );
      console.log("Data received: ", response.data);
    } catch (error) {
      console.log("Error: ", error);
    }
  };
  const handleCheckboxChange = () => {
    setUsePhotoAllowed(!usePhotoAllowed);
  };
  return (
    <div>
      <input
        text="Choose a WBDF TEM image"
        type="file"
        accept="image/*"
        display="none"
        onClick={handleFileChange}
      ></input>
      <button onClick={handleUpload}>Upload the chosen image</button>
      <input
        type="checkbox"
        id="allowUse"
        checked={usePhotoAllowed}
        onChange={handleCheckboxChange}
      ></input>
    </div>
  );
}

export default UploadPhotoFile;
