import React, { useEffect } from "react";
import "../App.css";

function CustomOutput(props) {
  const {
    labelText,
    inputID,
    inputAccuracy,
    defaultValue,
    hasInfoIcon,
    infoImageSource,
    allowChange,
  } = props;
  useEffect(() => {
    if (typeof window?.MathJax !== "undefined") {
      window.MathJax.typeset();
    }
  }, []);
  return (
    <div>
      <label for={inputID}>
        {labelText}
        {hasInfoIcon == "true" && infoImageSource != "" && (
          <img
            src={infoImageSource}
            alt="Info"
            style={{
              position: "relative",
              top: "-10px",
              heigh: "1.5em",
              width: "1.5em",
            }}
          />
        )}
      </label>
      <input
        type="number"
        id={inputID}
        defaultValue={defaultValue}
        disabled={allowChange}
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

export default CustomOutput;
