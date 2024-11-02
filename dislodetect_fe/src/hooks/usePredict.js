import { useState, useEffect } from "react";
import { useUpdateDisloDensity } from "./useUpdateDisloDensity";
import { useRemoveOldBB } from "./useRemoveOldBB";
import axios from "axios";

export function usePredict(props) {
  const [shouldPredict, setShouldPredict] = useState(false);
  const [predicting, setPredicting] = useState(false);
  const {
    predictRoboflow,
    imgContainerRef,
    photo,
    confidence,
    overlap,
    updateSumBhi,
  } = props;
  const triggerRemoveOldBB = useRemoveOldBB();
  const triggerUpdateDisloDensity = useUpdateDisloDensity();
  const triggerPredict = () => {
    setShouldPredict(true);
  };
  const predictBB = async () => {
    setPredicting(true);
    triggerRemoveOldBB();
    const formData = new FormData();
    //formData.append("file", photo);
    console.log(`confidence ${confidence}`);
    console.log(`overlap ${overlap}`);
    formData.append("confidence", parseFloat(confidence).toFixed(3));
    formData.append("overlap", parseFloat(overlap).toFixed(3));

    try {
      let requestURL = "";
      console.log("predictRoboflow: ", predictRoboflow);
      if (predictRoboflow === true) {
        requestURL = `${process.env.REACT_APP_DOTNET_API_URL}/predict`; //  http://localhost:5226
      } else {
        requestURL = `${process.env.REACT_APP_PYTHON_API_URL}/predict`; //"http://localhost:5000/predict"
      }
      const response = await axios.post(requestURL, formData);
      let results = response.data;
      // console.log("Data received: ", results);

      if (predictRoboflow) {
        results = formatResults(results);
      }
      console.log(`Formatted results: \n`, results);
      processBB(results, imgContainerRef);
    } catch (error) {
      console.log("Error why predicting: ", error);
    } finally {
      setPredicting(false);
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

  const bbColors = ["red"];
  function processBB(formattedResults, imgContainerRef) {
    // remove all the bounding boxes in the previous frames

    let bboxes = formattedResults[2];
    let SumBhi = 0;
    console.log(`bboxes: `, bboxes);

    let bbColor = "DarkRed"; // assign a dummy bounding box border colors
    const imgContainer = imgContainerRef.current;

    if (bboxes.length > 0) {
      for (let i = 0; i < bboxes.length; i++) {
        // drawing bounding boxes around the detected objects
        const htmlBoundingBox = document.createElement("div");
        htmlBoundingBox.className = "bounding-box";
        imgContainer.appendChild(htmlBoundingBox);
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
  useEffect(() => {
    if (shouldPredict) {
      predictBB();
      triggerUpdateDisloDensity();
      setShouldPredict(false);
    }
  }, [shouldPredict]);
  return { predicting, triggerPredict };
}
