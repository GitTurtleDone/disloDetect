import React, { useState, useEffect } from "react";
import "../App.css";
import InfoImage from "./InfoImage";

function CustomInput(props) {
  const {
    labelText,
    inputID,
    inputAccuracy,
    defaultValue,
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
  // useEffect(() => {
  //   const timeoutId = setTimeout(() => {
  //     setShowExplanationImage(false);
  //   }, 1000);
  //   return () => clearTimeout(timeoutId);
  // }, [showExplanationImage]);

  return (
    <div
      style={{
        marginBottom: "30px",
      }}
    >
      <label for={inputID}>
        {labelText}

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
        id={inputID}
        defaultValue={defaultValue}
        disabled={!(allowChange === "true")}
        style={{ marginLeft: "10px" }}
      ></input>

      {/* <label for="optSumBhi">Calculated sum of all relative bounding box heights &#8721<i>b</i><sub>hi</sub>:</label>
      <input
        type="number"
        id = "optSumBhi"
        defaultValue="3.46"
        disabled="true"
      ></input> */}
    </div>
  );
}

export default CustomInput;
