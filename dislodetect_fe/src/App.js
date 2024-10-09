import "./App.css";
import { useState } from "react";
import React, { useRef, useEffect } from "react";
import UploadPhotoFile from "./components/UploadPhotoFile";
import CustomInput from "./components/CustomInput";
import InputSlider from "./components/InputSlider";
import { useUpdateDisloDensity } from "./hooks/useUpdateDisloDensity";
import { usePredict } from "./hooks/usePredict";
import Infos from "./components/Infos.json";

function App() {
  const [photoFileSource, setPhotoFileSource] = useState();
  const [photoFile, setPhotoFile] = useState();
  const [sumBhi, setSumBhi] = useState(3.46);
  const imgContainerRef = useRef(null); // reference to the photo element
  const [confidence, setConfidence] = useState(0.254);
  const [overlap, setOverlap] = useState(0.7);
  const [predictRoboflow, setPredictRoboFlow] = useState(true);
  const [txtSwitchPredict, setTxtSwitchPredict] = useState(
    "Switch to Ultralytics"
  );

  const switchPredict = () => {
    // if (predictRoboflow) {
    //   setTxtSwitchPredict("Switch to Ultralytics")
    // } else {
    //   setTxtSwitchPredict("Switch to Roboflow")
    // }
    setPredictRoboFlow(!predictRoboflow);
  };
  const updatePhotoFileSource = (source) => {
    setPhotoFileSource(source);
  };

  const updateSumBhi = (data) => {
    setSumBhi(data);
    document.getElementById(customOutputInfos["sumBhi"]["iptId"]).value =
      data.toFixed(1);
    triggerUpdateDisloDensity();
  };

  const updatePhotoFile = (photoFile) => {
    setPhotoFile(photoFile);
  };

  const updateConfidence = (data) => {
    setConfidence(data);
    triggerPredict();
    console.log("confidence updated");
  };
  const updateOverlap = (data) => {
    setOverlap(data);
    triggerPredict();
    console.log("overlap updated");
  };
  const { predicting, triggerPredict } = usePredict({
    predictRoboflow,
    imgContainerRef,
    photoFile,
    confidence,
    overlap,
    updateSumBhi,
  });
  const triggerUpdateDisloDensity = useUpdateDisloDensity();

  const customOutputInfos = Infos[0];
  const inputSliderInfos = Infos[1];

  useEffect(() => {
    if (typeof window?.MathJax !== "undefined") {
      window.MathJax.typeset();
    }
  }, []);

  return (
    <div className="center-container">
      <h1>Dislocation Detection</h1>
      <div id="image-container" ref={imgContainerRef}>
        <img src={photoFileSource} alt="" />
      </div>
      <p>Select only weak beam dark field TEM images</p>
      <UploadPhotoFile updatePhotoFileSource={updatePhotoFileSource} />

      <div style={{ display: "grid", gridTemplateColumns: "250px 250px",justifyContent: "center" }}>
        <button
          onClick={triggerPredict}
          style={{
            backgroundColor: "#0055bb",
            width: "200px",
            height: "30px",
            marginBottom: "30px",
            color: "#ffffff",
            border: "none",
          }}
        >
          Predict ({predictRoboflow ? "Roboflow" : "Ultralytics"})
        </button>

        <button
          onClick={switchPredict}
          style={{
            backgroundColor: "#ffffff",
            borderColor: "#000000",
            width: "200px",
            height: "30px",
            marginBottom: "30px",
            color: "#000000",
            display: "inline-flex",
            alignItems: "center",
            justifyContent: "center",
            
            
          }}
        >
          <img
            src="SwitchIcon.png"
            style={{
              height: "1.5em",
              width: "1.5em",
              marginRight: "0.5em"
            }}
          />
          {predictRoboflow ? "Switch to Ultralytics" : "Switch to Roboflow"}
        </button>
      </div>
      {predicting && (
        <div className="overlay">
          <div className="overlay-content">
            <p>Predicting ....</p>
          </div>
        </div>
      )}

      {Object.keys(customOutputInfos).map((key) => {
        const value = customOutputInfos[key];

        return (
          <CustomInput
            key={key}
            lblText={value["lblText"]}
            iptId={value["iptId"]}
            iptIncrement={value["iptIncrement"]}
            dftVal={value["dftVal"]}
            hasInfoIcon={value["hasInfoIcon"]}
            explanationImageSource={value["explanationImageSource"]}
            allowChange={value["allowChange"]}
          />
        );
      })}
      {Object.keys(inputSliderInfos).map((key) => {
        const value = inputSliderInfos[key];

        return (
          <InputSlider
            key={key}
            iptText={value["iptText"]}
            iptId={value["iptId"]}
            sldId={value["sldId"]}
            iptStep={value["iptStep"]}
            sldStep={value["sldStep"]}
            dftVal={value["dftVal"]}
            updateVal={
              value["iptText"] == "Confidence"
                ? updateConfidence
                : updateOverlap
            }
          />
        );
      })}
      <footer>
        <p class="copyright">&copy; 2024 Giang T. Dang. All rights reserved.</p>
      </footer>
    </div>
  );
}

export default App;
