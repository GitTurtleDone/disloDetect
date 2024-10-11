import { useState} from "react";
// import { useUpdateDisloDensity } from "../hooks/useUpdateDisloDensity";
// import { usePredict } from "../hooks/usePredict";
// import Infos from "./Infos.json";
import InfoImage from "./InfoImage";
function PredictButton(props) {
  const {triggerPredict, updatePredictRoboflow} = props;
  const [predictRoboflow, setPredictRoboflow] = useState(true);
  const switchPredict = () => {
    updatePredictRoboflow(!predictRoboflow);
    setPredictRoboflow(!predictRoboflow);
  }

  return (
    <div
      style={{
        display: "grid",
        gridTemplateColumns: "210px 210px 40px",
        justifyContent: "center",
      }}
    >
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
            marginRight: "0.5em",
          }}
        />
        {predictRoboflow ? "Switch to Ultralytics" : "Switch to Roboflow"}
        
      </button>
      <InfoImage explanationImageSrc={"ExplainPredictMode.jpg"} />
      
    </div>
  );
}
export default PredictButton;
