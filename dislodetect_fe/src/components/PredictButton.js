import React from "react";
import axios from "axios";
import "../App.css";
import { useRemoveOldBB } from "../hooks/useRemoveOldBB.js";
import { usePredict } from "../hooks/usePredict.js";

function Predict(props) {
  const { predicting, triggerPredict } = usePredict(props);
  // const {imgContainerRef, photo, confidence, overlap, updateSumBhi} = props;
  // console.log(`confidence: ${confidence}`);
  // const { imgContainerRef, photo, updateSumBhi } = props;
  // const triggerRemoveOldBB = useRemoveOldBB();
  // const predictBB = async () => {
  //   triggerRemoveOldBB();
  //   const formData = new FormData();
  //   formData.append("file", photo);
  //   try {
  //     const response = await axios.post(
  //       "http://localhost:5226/Predict",
  //       formData
  //     );
  //     let results = response.data;
  //     console.log("Data received: ", results);
  //     let formattedResults = formatResults(results);
  //     console.log(`Formatted results: \n`, formattedResults);
  //     processBB(formattedResults, imgContainerRef);
  //   } catch (error) {
  //     console.log("Error why predicting: ", error);
  //   }
  // };

  // function formatResults(results) {
  //   let bboxes = [];
  //   let imageWidth = results["image"]["width"];
  //   let imageHeight = results["image"]["height"];
  //   results["predictions"].forEach((bb) => {
  //     console.log(bb);
  //     bboxes.push([
  //       (bb["x"] / imageWidth) * 100,
  //       (bb["y"] / imageHeight) * 100,
  //       (bb["width"] / imageWidth) * 100,
  //       (bb["height"] / imageHeight) * 100,
  //       bb["confidence"],
  //     ]);
  //   });
  //   return [results["image"]["width"], results["image"]["height"], bboxes];
  // }

  // const bbColors = ["red"];
  // function processBB(formattedResults, imgContainerRef) {
  //   // remove all the bounding boxes in the previous frames

  //   let bboxes = formattedResults[2];
  //   let SumBhi = 0;
  //   console.log(`bboxes: `, bboxes);

  //   let bbColor = "DarkRed"; // assign a dummy bounding box border colors
  //   const imgContainer = imgContainerRef.current;

  //   if (bboxes.length > 0) {
  //     for (let i = 0; i < bboxes.length; i++) {
  //       // drawing bounding boxes around the detected objects
  //       const htmlBoundingBox = document.createElement("div");
  //       htmlBoundingBox.className = "bounding-box";
  //       imgContainer.appendChild(htmlBoundingBox);
  //       // console.log("bbColor Index", bboxes[0][i]);
  //       htmlBoundingBox.style.left = `${bboxes[i][0] - bboxes[i][2] / 2}%`;
  //       htmlBoundingBox.style.top = `${bboxes[i][1] - bboxes[i][3] / 2}%`;
  //       htmlBoundingBox.style.width = `${bboxes[i][2]}%`;
  //       htmlBoundingBox.style.height = `${bboxes[i][3]}%`;
  //       htmlBoundingBox.style.borderColor = bbColor;
  //       SumBhi += bboxes[i][3] / 100;
  //       // drawing bounding boxes around the detected objects
  //     }
  //   }
  //   updateSumBhi(SumBhi);
  //   console.log(`Sum of bhi: `, parseFloat(SumBhi).toFixed(2));
  // }

  return (
    <div style={{marginBottom: "30px"}}>
      <button onClick={triggerPredict}>Predict (roboflow)</button>
    </div>
  );
}

export default Predict;
