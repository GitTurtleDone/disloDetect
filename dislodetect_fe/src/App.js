// import logo from './logo.svg';
import "./App.css";
import { useState } from "react";
import UploadPhotoFile from "./components/UploadPhotoFile";
function App() {
  const [photoFileSource, setPhotoFileSource] = useState();
  const updatePhotoFileSource = (source) => {
    setPhotoFileSource(source);
    console.log("Photo File Source: ", source);
    // const previewImage = document.getElementById("photo");
    // previewImage.source = source;
    // previewImage.style.display = "block";
  };
  const imagePath = "../../Public/SavedImages/Ref03_Fig4b_Rot45.jpg";
  return (
    <div>
      <h1>Dislocation Detection</h1>
      <div id="image-container">
        <img src={photoFileSource} alt="" />
      </div>
      <UploadPhotoFile updatePhotoFileSource={updatePhotoFileSource} />
    </div>
  );
}

export default App;
