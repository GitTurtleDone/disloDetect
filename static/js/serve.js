const saveFolderPath = "../Public/SavedImages/";
let uploadedFileName = "";

document
  .getElementById("btnUploadPhoto")
  .addEventListener("click", async function () {
    removeOldBB();
    var fileInput = document.getElementById("iptUploadPhoto");
    var previewImage = document.getElementById("photo");
    // Check if file is selected
    if (fileInput.files && fileInput.files[0]) {
      var reader = new FileReader();

      // Read the image file as a data URL
      reader.onload = function (e) {
        // Set the preview image source to the data URL
        previewImage.src = e.target.result;
        previewImage.style.display = "block"; // Show the image
        var formData = new FormData();
        uploadedFileName = fileInput.files[0].name;
        console.log(`The file ${uploadedFileName} has been uploaded"`);
        savePhotoToServer(fileInput.files[0]);
      };

      // Load image file as a data URL
      reader.readAsDataURL(fileInput.files[0]);
    }
  });

function savePhotoToServer(photoFile) {
  var formData = new FormData();
  formData.append("photo", photoFile);

  // Send HTTP POST request to server
  fetch("/savePhoto", {
    method: "POST",
    body: formData,
  })
    .then((response) => {
      if (response.ok) {
        console.log("Image uploaded successfully");
      } else {
        console.error("Failed to upload image");
      }
    })
    .catch((error) => {
      console.error("Error:", error);
    });
}
// document
//   .getElementById("btnPredict")
//   .addEventListener("click", async function () {
//     try {
//       const response = await fetch("/predict");
//       const bboxes = await response.json();
//       console.log(`prediction results: \n`, bboxes);
//       returnInfo = processBB(bboxes);
//     } catch (error) {
//       console.error(error);
//     }
//   });

document
  .getElementById("btnPredict")
  .addEventListener("click", async function () {
    predict();
  });

async function predict() {
  try {
    removeOldBB();
    const confidence = document.getElementById("iptConfidence").value;
    const overlap = document.getElementById("iptOverlap").value;
    const response = await fetch("/predict", {
      method: "POST",
      body: JSON.stringify({ confidence, overlap }),
      headers: { "Content-Type": "application/json" },
    });
    const results = await response.json();
    console.log(`results: \n`, results);
    formattedResults = formatResults(results);
    console.log(`Formatted results: \n`, formattedResults);
    processBB(formattedResults);
    //   returnInfo = processBB(bboxes);
  } catch (error) {
    console.error(error);
  }
}

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
  bboxes = formattedResults[2];
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
      // if (i < 3) {
      //   console.log(`bb ${i} \
      //             left: ${htmlBoundingBox.style.left} \
      //             top: ${htmlBoundingBox.style.top} \
      //             width: ${htmlBoundingBox.style.width} \
      //             height: ${htmlBoundingBox.style.height} \
      //              `);
      // }
      SumBhi += bboxes[i][3] / 100;
      // drawing bounding boxes around the detected objects
    }
  }
  document.getElementById("txtSumBhi").value = parseFloat(SumBhi).toFixed(2);
  console.log(`Sum of bhi: `, parseFloat(SumBhi).toFixed(2));
  return "predicted";
}

// function updateSlider(strSldID, strSldDisplay, strFlaskEndPoint) {
//   const slider = document.getElementById(strSldID);
//   const valueDisplay = document.getElementById(strSldDisplay);
//   const newValue = slider.value;
//   valueDisplay.textContent = newValue;
//   // Send the new value to the Flask server using AJAX
//   // fetch(strFlaskEndPoint, {
//   //   method: "POST",
//   //   body: JSON.stringify({ value: newValue }),
//   // })
//   //   .then((response) => response.json())
//   //   .then((data) => {
//   //     console.log("Server response:", data);
//   //   })
//   //   .catch((error) => console.error(error));
// }

// function updateConfidence() {
//   updateSlider("sldConfidence", "valueConfidenceDisplay", "/setConfidence");
//   console.log(
//     "Confidence set to: ",
//     document.getElementById("valueConfidenceDisplay").textContent
//   );

//   predict1();
// }
// function updateOverlap() {
//   updateSlider("sldOverlap", "valueOverlapDisplay", "/setIoU");
//   console.log(
//     "IoU set to: ",
//     document.getElementById("valueOverlapDisplay").textContent
//   );
//   predict1();
// }

const inputSliders = document.querySelectorAll(".inputSlider");

inputSliders.forEach((pair) => {
  const ipt = pair.querySelector('input[type="number"]');
  const slider = pair.querySelector('input[type="range"]');

  function updateSlider() {
    const newValue = parseFloat(ipt.value).toFixed(3);
    slider.value = newValue;
    // console.log(`updated value: ${newValue}`);
    predict();
  }

  function updateInput() {
    const newValue = parseFloat(slider.value).toFixed(3);
    ipt.value = newValue;
    // console.log(`updated value: ${newValue}`);
    predict();
  }

  ipt.addEventListener("change", updateSlider);
  slider.addEventListener("change", updateInput);

  // Optional initial update
  // updateSlider();
});

window.onload = async function () {};
