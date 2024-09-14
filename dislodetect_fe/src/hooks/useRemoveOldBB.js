import { useState, useEffect } from "react";
export function useRemoveOldBB() {
  const [shouldRemoveOldBB, setShouldRemoveOldBB] = useState(false);
  const triggerRemoveOldBB = () => {
    setShouldRemoveOldBB(true);
  };
  useEffect(() => {
    if (shouldRemoveOldBB) {
      const oldBBoxes = document.getElementsByClassName("bounding-box");
      while (oldBBoxes.length > 0) {
        oldBBoxes[0].parentNode.removeChild(oldBBoxes[0]);
      }
      setShouldRemoveOldBB(false);
    }
  }, [shouldRemoveOldBB]);

  // console.log(`went in removeOldBB`);
  return triggerRemoveOldBB;
}
