// import logo from './logo.svg';
import "./App.css";
import { useState } from "react";
import UploadPhotoFile from "./components/UploadPhotoFile";
import Predict from "./components/PredictButton";
function App() {
  const [photoFileSource, setPhotoFileSource] = useState();
  const [photoFile, setPhotoFile] = useState();
  const [sumBhi, setSumBhi] = useState(3.46);
  const updatePhotoFileSource = (source) => {
    setPhotoFileSource(source);
    console.log("Photo File Source: ", source);
    // const previewImage = document.getElementById("photo");
    // previewImage.source = source;
    // previewImage.style.display = "block";
  };

  const updateSumBhi = (data) => {
    setSumBhi(data);
  };

  const updatePhotoFile = (photoFile) => {
    setPhotoFile(photoFile);
  };
  // const imagePath = "../../Public/SavedImages/Ref03_Fig4b_Rot45.jpg";
  return (
    <div>
      <h1>Dislocation Detection</h1>
      <div id="image-container">
        <img id="disloImage" src={photoFileSource} alt="" />
      </div>
      <UploadPhotoFile updatePhotoFileSource={updatePhotoFileSource} />
      <Predict photo={photoFile} updateSumBhi={updateSumBhi} />
    </div>
  );
}

export default App;
