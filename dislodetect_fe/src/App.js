// import logo from './logo.svg';
import "./App.css";
import { useState } from "react";
import React, { useRef } from "react";
import UploadPhotoFile from "./components/UploadPhotoFile";
import Predict from "./components/PredictButton";
import { useRemoveOldBB } from "./hooks/useRemoveOldBB";
function App() {
  const [photoFileSource, setPhotoFileSource] = useState();
  const [photoFile, setPhotoFile] = useState();
  const [sumBhi, setSumBhi] = useState(3.46);
  const imgContainerRef = useRef(null); // reference to the photo element
  const updatePhotoFileSource = (source) => {
    setPhotoFileSource(source);
  };

  const updateSumBhi = (data) => {
    setSumBhi(data);
  };

  const updatePhotoFile = (photoFile) => {
    setPhotoFile(photoFile);
  };
  // const imagePath = "../../Public/SavedImages/Ref03_Fig4b_Rot45.jpg";
  return (
    <div>
      <h1>Dislocation Detection</h1>
      <div id="image-container" ref={imgContainerRef}>
        <img src={photoFileSource} alt="" />
      </div>
      <UploadPhotoFile
        updatePhotoFileSource={updatePhotoFileSource}
        // triggerRemoveOldBB={triggerRemoveOldBB}
      />
      <Predict
        imgContainerRef={imgContainerRef}
        photo={photoFile}
        updateSumBhi={updateSumBhi}
        // triggerRemoveOldBB={triggerRemoveOldBB}
      />
    </div>
  );
}

export default App;
