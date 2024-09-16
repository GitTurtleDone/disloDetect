import { useState, useEffect } from "react";
import Infos from "../components/Infos.json";

export function useResetInputs() {
  //   const dftConf = 0.254; // default Confidence
  //   const dftIoU = 0.7; // default overlap IoU
  //   const dftSumBhi = 3.46; // default sum of BhI
  //   const dftDisloDensity = 3.3e10; // default dislocation density
  //   const dftAf = 8.6e4; // default film area
  //   const dft_t = 100; // default TEM specimen thickness
  //   const dftHimage = 825; // default height of the entire image
  const dftCustomInputValues = ["3.46", "3.3e10", "8.6e4", "100", 85];
  const dftInputSliderValues = [0.254, 0.7];
  const customOutputInfos = Infos[0];
  const inputSliderInfos = Infos[1];
  const [shouldResetInputs, setShouldResetInputs] = useState(false);
  const triggerResetInputs = () => {
    setShouldResetInputs(true);
  };
  useEffect(() => {
    if (shouldResetInputs) {
      console.log("customOutputInfos", customOutputInfos);
      console.log("inputSliderInfos", inputSliderInfos);
      Object.keys(customOutputInfos).map((key, index) => {
        const value = customOutputInfos[key];
        console.log(value["dftVal"]);
        document.getElementById(value["iptId"]).value =
          parseFloat(value["dftVal"]) < 1000 ? parseFloat(value["dftVal"]) : parseFloat(value["dftVal"]).toPrecision(2);
      });
      Object.keys(Infos[1]).map((key, index) => {
        const value = inputSliderInfos[key];
        // console.log("value slider: ", value["dftVal"]);
        document.getElementById(value["iptId"]).value = parseFloat(value["dftVal"]);
        document.getElementById(value["sldId"]).value = parseFloat(value["dftVal"]);
      });
      setShouldResetInputs(false);
    }
  }, [shouldResetInputs]);

  return triggerResetInputs;
}
