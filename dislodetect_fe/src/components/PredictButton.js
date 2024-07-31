import React, { useState } from "react";
import axios, { AxiosResponse } from "axios";
function Predict(props) {
  const { photo, updateSumBhi } = props;
  const predictBB = async () => {
    const formData = new FormData();
    formData.append("file", photo);
    try {
      const response = await axios.post(
        "http://localhost:5226/Predict",
        formData
      );
      let results = response.data;
      console.log("Data received: ", results);
      let formattedResults = formatResults(results);
      console.log(`Formatted results: \n`, formattedResults);
      processBB(formattedResults);
    } catch (error) {
      console.log("Error why predicting: ", error);
    }
  };

  function formatResults(results) {
    let bboxes = [];
    let imageWidth = results["image"]["width"];
    let imageHeight = results["image"]["height"];
    results["predictions"].forEach((bb) => {
      console.log(bb);
      bboxes.push([
        (bb["x"] / imageWidth) * 100,
        (bb["y"] / imageHeight) * 100,
        (bb["width"] / imageWidth) * 100,
        (bb["height"] / imageHeight) * 100,
        bb["confidence"],
      ]);
    });
    return [results["image"]["width"], results["image"]["height"], bboxes];
  }
  function removeOldBB() {
    const oldBBoxes = document.getElementsByClassName("bounding-box");
    while (oldBBoxes.length > 0) {
      oldBBoxes[0].parentNode.removeChild(oldBBoxes[0]);
    }
  }
  const bbColors = ["red"];
  function processBB(formattedResults) {
    // remove all the bounding boxes in the previous frames
    let bboxes = formattedResults[2];
    let SumBhi = 0;
    console.log(`bboxes: `, bboxes);
    removeOldBB();
    let bbColor = "DarkRed"; // assign a dummy bounding box border colors
    if (bboxes.length > 0) {
      for (let i = 0; i < bboxes.length; i++) {
        // drawing bounding boxes around the detected objects
        const htmlBoundingBox = document.createElement("div");
        htmlBoundingBox.className = "bounding-box";
        document.getElementById("image-container").appendChild(htmlBoundingBox);
        // console.log("bbColor Index", bboxes[0][i]);
        htmlBoundingBox.style.left = `${bboxes[i][0] - bboxes[i][2] / 2}%`;
        htmlBoundingBox.style.top = `${bboxes[i][1] - bboxes[i][3] / 2}%`;
        htmlBoundingBox.style.width = `${bboxes[i][2]}%`;
        htmlBoundingBox.style.height = `${bboxes[i][3]}%`;
        htmlBoundingBox.style.borderColor = bbColor;
        SumBhi += bboxes[i][3] / 100;
        // drawing bounding boxes around the detected objects
      }
    }
    updateSumBhi(SumBhi);
    console.log(`Sum of bhi: `, parseFloat(SumBhi).toFixed(2));
  }

  return (
    <div>
      <button onClick={predictBB}>Predict (roboflow)</button>
    </div>
  );
}

export default Predict;
