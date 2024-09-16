import { useState, useEffect } from "react";
export function useUpdateDisloDensity() {
  const [shouldUpdateDisloDensity, setShouldUpdateDisloDensity] =
    useState(false);
  
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
