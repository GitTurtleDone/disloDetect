import { useState, useEffect } from "react";
export function useUpdateDisloDensity() {
  const [shouldUpdateDisloDensity, setShouldUpdateDisloDensity] =
    useState(false);
  const dftConf = 0.254; // default Confidence
  const dftIoU = 0.7; // default overlap IoU
  const dftSumBhi = 3.46; // default sum of BhI
  const dftDisloDensity = 3.3e10; // default dislocation density
  const dftAf = 8.6e4; // default film area
  const dft_t = 100; // default TEM specimen thickness
  const dftHimage = 825; // default height of the entire image
  const triggerUpdateDisloDensity = () => {
    setShouldUpdateDisloDensity(true);
  };
  useEffect(() => {
    if (shouldUpdateDisloDensity) {
      const disloDensity = document.getElementById("optDisloDensity");
      var density =
        (document.getElementById("optSumBhi").value *
          document.getElementById("iptImageHeight").value *
          1e14) /
        (document.getElementById("iptFilmArea").value *
          document.getElementById("iptTEMSpecimenThickness").value);
      disloDensity.value = density.toPrecision(2);
      setShouldUpdateDisloDensity(false);
    }
  }, [shouldUpdateDisloDensity]);
  return triggerUpdateDisloDensity;
}
