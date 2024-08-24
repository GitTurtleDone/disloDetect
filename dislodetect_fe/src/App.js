// import logo from './logo.svg';
import "./App.css";
import { useState } from "react";
import React, { useRef, useEffect } from "react";
import UploadPhotoFile from "./components/UploadPhotoFile";
import Predict from "./components/PredictButton";
import { useRemoveOldBB } from "./hooks/useRemoveOldBB";
import CustomOutput from "./components/CustomOutput";

function App() {
  const [photoFileSource, setPhotoFileSource] = useState();
  const [photoFile, setPhotoFile] = useState();
  const [sumBhi, setSumBhi] = useState(3.46);
  const imgContainerRef = useRef(null); // reference to the photo element
  const lblSumBhi =
    "Calculated sum of all relative bounding box heights \\(\\sum b_{hi}\\): ";
  const lblAf = "Insert the film area \\(A_f\\) \\((nm^2)\\): ";

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
  useEffect(() => {
    if (typeof window?.MathJax !== "undefined") {
      window.MathJax.typeset();
    }
  }, []);
  const mathEq =
    "Calculated sum of all relative bounding box heights \\(\\sum\\)";
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
      {/* <p>This is my equation: {mathEq}</p> */}
      <CustomOutput
        labelText={lblAf}
        inputID="optBhi"
        inputAccuracy="1e-2"
        defaultValue="3.46"
        hasInfoIcon="true"
        infoImageSource="InfoIcon.png"
        allowChange="false"
      />
    </div>
  );
}

export default App;
