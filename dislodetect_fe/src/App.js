// import logo from './logo.svg';
import "./App.css";
import { useState } from "react";
import React, { useRef, useEffect } from "react";
import UploadPhotoFile from "./components/UploadPhotoFile";
import Predict from "./components/PredictButton";
import { useRemoveOldBB } from "./hooks/useRemoveOldBB";
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
  // const [customOutputInfos, setCustomOutputInfos] = useState(null);
  // const [inputSliderInfos, setInputSliderInfos] = useState(null);
  // const lblSumBhi =
  //   "Calculated sum of all relative bounding box heights \\(\\sum b_{hi}\\): ";
  // const lblAf = "Insert the film area \\(A_f\\) \\((nm^2)\\): ";

  // const customOutputInfos = Infos[0];
  // const inputSliderInfos = Infos[1];
  // useEffect(()=>{
  //   const fetchData = async () =>{
  //     try {
  //       const response = await fetch('./components/Infos.json');
  //       const data = await response.json();
  //       const jsonData = JSON.stringify(data);
  //       setCustomOutputInfos(jsonData["customOutputInfos"]);
  //       setInputSliderInfos(jsonData["inputSliderInfos"]);
  //       console.log("customOutpuInfos: ", customOutputInfos);
  //       console.log("inputSliderInfo: ", inputSliderInfos);
  //     } catch (error) {
  //       console.log(error.messag);
  //     }
  //   };
  //   fetchData();
  // },[]);

  const updatePhotoFileSource = (source) => {
    setPhotoFileSource(source);
  };

  const updateSumBhi = (data) => {
    setSumBhi(data);
    document.getElementById(customOutputInfos["sumBhi"]["inputID"]).value =
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
    imgContainerRef,
    photoFile,
    confidence,
    overlap,
    updateSumBhi,
  });
  const triggerUpdateDisloDensity = useUpdateDisloDensity();

  const customOutputInfos = {
    sumBhi: {
      labelText:
        "Calculated sum of all relative bounding box heights \\(\\sum b_{hi}\\): ",
      inputID: "optSumBhi",
      inputIncrement: "1e-1",
      defaultValue: "3.46",
      hasInfoIcon: "false",
      explanationImageSource: "",
      allowChange: "false",
    },

    DisloDensity: {
      labelText:
        "Calculated dislocation density \\(\\rho = \\sum b_{hi}H_{image}/(A_f t)\\) \\((cm^{-2})\\): ",
      inputID: "optDisloDensity",
      inputIncrement: "1e9",
      defaultValue: "3.3e+10",
      hasInfoIcon: "false",
      explanationImageSource: "",
      allowChange: "false",
    },

    Af: {
      labelText: "Insert the film area \\(A_f\\) \\((nm^2)\\): ",
      inputID: "iptFilmArea",
      inputIncrement: "1e4",
      defaultValue: "8.6e4",
      hasInfoIcon: "true",
      explanationImageSource: "ParamExplainAf.jpg",
      allowChange: "true",
    },

    t: {
      labelText:
        "Insert the thickness of the TEM specimen \\(t\\) \\((nm)\\): ",
      inputID: "iptTEMSpecimenThickness",
      inputIncrement: "10",
      defaultValue: "100",
      hasInfoIcon: "true",
      explanationImageSource: "ParamExplain_t.jpg",
      allowChange: "true",
    },

    Himage: {
      labelText:
        "Insert the height of the whole image \\(H_{image}\\) \\((nm)\\): ",
      inputID: "iptImageHeight",
      inputIncrement: "10",
      defaultValue: "85",
      hasInfoIcon: "true",
      explanationImageSource: "ParamExplainHimage.jpg",
      allowChange: "true",
    },
  };
  const inputSliderInfos = {
    iptSldConfidence: {
      iptText: "Confidence",
      iptId: "iptConfidence",
      sldId: "sldConfidence",
      iptStep: "0.01",
      sldStep: "0.001",
      dftValue: "0.254",
    },
    iptSldOverlap: {
      iptText: "Overlap",
      iptId: "iptOverlap",
      sldId: "sldOverlap",
      iptStep: "0.01",
      sldStep: "0.001",
      dftValue: "0.7",
    },
  };

  // const imagePath = "../../Public/SavedImages/Ref03_Fig4b_Rot45.jpg";
  console.log(customOutputInfos);
  useEffect(() => {
    if (typeof window?.MathJax !== "undefined") {
      window.MathJax.typeset();
    }
  }, []);
  // const mathEq =
  //   "Calculated sum of all relative bounding box heights \\(\\sum\\)";
  const value = customOutputInfos["Af"];
  return (
    <div className="center-container">
      <h1>Dislocation Detection</h1>
      <div id="image-container" ref={imgContainerRef}>
        <img src={photoFileSource} alt="" />
      </div>
      <p>Select only weak beam dark field TEM images</p>
      <UploadPhotoFile
        updatePhotoFileSource={updatePhotoFileSource}
        // triggerRemoveOldBB={triggerRemoveOldBB}
      />
      {/* <Predict
        imgContainerRef={imgContainerRef}
        photo={photoFile}
        confidence={confidence}
        overlap={overlap}
        updateSumBhi={updateSumBhi}
        // triggerRemoveOldBB={triggerRemoveOldBB}
      /> */}
      <button
        onClick={triggerPredict}
        style={{
          backgroundColor: "#0055bb",
          width: "150px",
          height: "30px",
          marginBottom: "30px",
          color: "#ffffff",
          border: "none",
        }}
      >
        Predict (roboflow)
      </button>
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
            labelText={value["labelText"]}
            inputID={value["inputID"]}
            inputIncrement={value["inputIncrement"]}
            defaultValue={value["defaultValue"]}
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
            dftValue={value["dftValue"]}
            updateValue={
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
