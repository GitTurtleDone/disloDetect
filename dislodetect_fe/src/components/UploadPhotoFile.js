import React, { useState } from "react";
import axios, { AxiosResponse } from "axios";
import { useRemoveOldBB } from "../hooks/useRemoveOldBB.js";
import { useResetInputs } from "../hooks/useResetInputs.js";
function UploadPhotoFile({ updatePhotoFileSource }) {
  const triggerRemoveOldBB = useRemoveOldBB();
  const triggerResetInputs = useResetInputs();
  // const { updatePhotoFileSource} = props;
  const [photoFile, setPhotoFile] = useState(null);
  const [photoFileURL, setPhotoFileURL] = useState();
  const [usePhotoAllowed, setUsePhotoAllowed] = useState(false);
  const handleFileChange = (e) => {
    // const selectedPhoto = e.target.files[0];
    // const selectedPhotoFileName = selectedPhoto.name;
    setPhotoFile(e.target.files[0]);
    setPhotoFileURL(URL.createObjectURL(e.target.files[0]));
    console.log("In Upload Photo File: ", photoFileURL);
  };
  const handleUpload = async () => {
    triggerRemoveOldBB();
    triggerResetInputs();
    console.log("Before sending photo file name: ", photoFile);
    const formData = new FormData();
    formData.append("file", photoFile);
    formData.append("usePhotoAllowed", usePhotoAllowed);
    try {
      const response = await axios.post(
        `${process.env.REACT_APP_DOTNET_API_URL}/UploadPhotoFile`,
        formData
      );
      console.log("Data received: ", response.data);
      updatePhotoFileSource(photoFileURL);
      triggerRemoveOldBB();
    } catch (error) {
      console.log("Error: ", error);
    }
  };
  const handleCheckboxChange = () => {
    setUsePhotoAllowed(!usePhotoAllowed);
  };
  return (
    <div
      style={{
        display: "grid",
        gridTemplateRows: "auto",
        rowGap: "15px",
        justifyItems: "start",

        marginBottom: "30px",
      }}
    >
      <input
        text="Choose a WBDF TEM image"
        type="file"
        accept="image/*"
        display="none"
        onChange={handleFileChange}
      ></input>
      <button onClick={handleUpload}>Upload the chosen image</button>
      <div>
        <input
          type="checkbox"
          id="allowUse"
          checked={usePhotoAllowed}
          onChange={handleCheckboxChange}
        ></input>
        <label htmlFor="allowUse">
        Allow to use the uploaded photo to improve model training
        </label>
      </div>
    </div>
  );
}

export default UploadPhotoFile;
