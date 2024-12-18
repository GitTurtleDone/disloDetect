import React, { useState, useEffect } from "react";
import "../App.css";
import InfoImage from "./InfoImage";
import { useUpdateDisloDensity } from "../hooks/useUpdateDisloDensity";

function CustomInput(props) {
  const {
    lblText,
    iptId,
    iptIncrement,
    dftVal,
    hasInfoIcon,
    explanationImageSource,
    allowChange,
  } = props;
  const [showExplanationImage, setShowExplanationImage] = useState(false);
  useEffect(() => {
    if (typeof window?.MathJax !== "undefined") {
      window.MathJax.typeset();
    }
  }, []);
  const handleHoverOrClick = () => {
    setShowExplanationImage(!showExplanationImage);
  };

  const triggerUpdateDisloDensity = useUpdateDisloDensity();
  // useEffect(() => {
  //   const timeoutId = setTimeout(() => {
  //     setShowExplanationImage(false);
  //   }, 1000);
  //   return () => clearTimeout(timeoutId);
  // }, [showExplanationImage]);

  return (
    <div
      style={{
        display: "grid",
        gridTemplateColumns: "500px 10px",
        justifyItems: "start",
        justifyContent: "center",
        marginBottom: "30px",
      }}
    >
      <label for={iptId}>
        {lblText}

        {hasInfoIcon == "true" && explanationImageSource != "" && (
          // (
          //   <InfoImage explanationImageSrc="ParamExplainAf.jpg" />
          // )

          <img
            src="InfoIcon.png"
            alt="Info"
            onClick={handleHoverOrClick}
            onMouseEnter={handleHoverOrClick}
            style={{
              position: "relative",
              top: "-10px",
              height: "1.5em",
              width: "1.5em",
            }}
          />
        )}
        {showExplanationImage && (
          <img
            src={explanationImageSource}
            alt="Explanation"
            onClick={handleHoverOrClick}
            onMouseLeave={handleHoverOrClick}
            style={{
              position: "relative",
              top: "0",
              left: "0",
              display: "inline",
              zIndex: "9999",
            }}
          />
        )}
      </label>
      <input
        type="number"
        id={iptId}
        defaultValue={dftVal}
        disabled={!(allowChange === "true")}
        style={{ marginLeft: "10px" }}
        step={iptIncrement}
        onChange={triggerUpdateDisloDensity}
      ></input>

      {/* <label for="optSumBhi">Calculated sum of all relative bounding box heights &#8721<i>b</i><sub>hi</sub>:</label>
      <input
        type="number"
        id = "optSumBhi"
        dftVal="3.46"
        disabled="true"
      ></input> */}
    </div>
  );
}

export default CustomInput;
