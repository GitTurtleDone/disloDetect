// import logo from './logo.svg';
import "./App.css";
import { useState } from "react";
import React, { useRef, useEffect } from "react";
import UploadPhotoFile from "./components/UploadPhotoFile";
import Predict from "./components/PredictButton";
import { useRemoveOldBB } from "./hooks/useRemoveOldBB";
import CustomInput from "./components/CustomInput";

function App() {
  const [photoFileSource, setPhotoFileSource] = useState();
  const [photoFile, setPhotoFile] = useState();
  const [sumBhi, setSumBhi] = useState(3.46);
  const imgContainerRef = useRef(null); // reference to the photo element
  const lblSumBhi =
    "Calculated sum of all relative bounding box heights \\(\\sum b_{hi}\\): ";
  const lblAf = "Insert the film area \\(A_f\\) \\((nm^2)\\): ";

  const customOutputInfos = {
    sumBhi: {
      labelText:
        "Calculated sum of all relative bounding box heights \\(\\sum b_{hi}\\): ",
      inputID: "optSumBhi",
      inputAccuracy: "1e-2",
      defaultValue: "3.46",
      hasInfoIcon: "false",
      explanationImageSource: "",
      allowChange: "false",
    },

    DisloDensity: {
      labelText:
        "Calculated dislocation density \\(\\rho = \\sum b_{hi}H_{image}/(A_f t)\\) \\((cm^{-2})\\): ",
      inputID: "optDisloDensity",
      inputAccuracy: "1e-2",
      defaultValue: "3.3e+10",
      hasInfoIcon: "false",
      explanationImageSource: "",
      allowChange: "false",
    },

    Af: {
      labelText: "Insert the film area \\(A_f\\) \\((nm^2)\\): ",
      inputID: "iptFilmArea",
      inputAccuracy: "1e-2",
      defaultValue: "8.6e4",
      hasInfoIcon: "true",
      explanationImageSource: "ParamExplainAf.jpg",
      allowChange: "true",
    },

    t: {
      labelText:
        "Insert the thickness of the TEM specimen \\(t\\) \\((nm)\\): ",
      inputID: "iptTEMSpecimenThickness",
      inputAccuracy: "1e-2",
      defaultValue: "100",
      hasInfoIcon: "true",
      explanationImageSource: "ParamExplain_t.jpg",
      allowChange: "true",
    },

    Himage: {
      labelText:
        "Insert the height of the whole image \\(H_{image}\\) \\((nm)\\): ",
      inputID: "iptImageHeight",
      inputAccuracy: "1e-2",
      defaultValue: "85",
      hasInfoIcon: "true",
      explanationImageSource: "ParamExplainHimage.jpg",
      allowChange: "true",
    },
  };

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
  const value = customOutputInfos["Af"];
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
      
      {Object.keys(customOutputInfos).map((key) => {
        const value = customOutputInfos[key];
        return (
          <CustomInput
          key={key}
          labelText={value["labelText"]}
          inputID={value["inputID"]}
          inputAccuracy={value["inputAccuracy"]}
          defaultValue={value["defaultValue"]}
          hasInfoIcon={value["hasInfoIcon"]}
          explanationImageSource={value["explanationImageSource"]}
          allowChange={value["allowChange"]}
        />
        )
        
      })}
      <footer>
        <p class="copyright">&copy; 2024 Giang T. Dang. All rights reserved.</p>
      </footer>
    </div>
  );
}

export default App;
