import React, {useState} from "react";
function InfoImage({explanationImageSrc}) {
  const [showExplanationImage, setShowExplanationImage] = useState(false);
  const handleHoverOrClick = () =>{
    setShowExplanationImage(!showExplanationImage);
  };
  return (
    <div > 
      <img src="InfoIcon.png" style={{
        position: "relative",
        top: "-10px",
        height: "1.5em",
        width: "1.5em"
       }} onClick={handleHoverOrClick} onMouseEnter={handleHoverOrClick} onMouseLeave={handleHoverOrClick}/>
       {showExplanationImage && 
       <img src={explanationImageSrc} alt="Explanation" style={{
        position: "absolute",
        top: "0",
        left: "0",
       }}
       />}
    </div>
  );
}
export default InfoImage;
