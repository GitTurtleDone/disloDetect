import { useState } from "react";
function InputSlider(props) {
  const { iptText, iptId, sldId, iptStep, sldStep, dftValue } = props;
  const [value, setValue] = useState(parseFloat(dftValue));
  const updateSlider = (e) => {
    setValue(e.target.value);
  };
  const updateInput = (e) => {
    const iptVal = e.target.value;
    if (!isNaN(iptVal) && iptVal >= 0 && iptVal <= 1) {
      setValue(Number(iptVal));
    }
  };
  //   console.log(`iptText ${iptText}`);

  return (
    <div
      style={{
        display: "grid",
        gridTemplateColumns: "100px 60px 20px 150px 20px",
        justifyContent: "center",
      }}
    >
      <label
        for={iptId}
        style={{ justifySelf: "left", marginRight: "10px", textAlign: "left" }}
      >
        {iptText}
      </label>
      <input
        type="number"
        id={iptId}
        min="0"
        max="1"
        step={iptStep}
        value={value}
        disabled={false}
        onChange={updateSlider}
      />
      <label for={sldId} style={{ marginLeft: "8px", marginRight: "2px" }}>
        0
      </label>
      <input
        type="range"
        min="0"
        max="1"
        step={sldStep}
        value={value}
        disabled={false}
        onChange={updateInput}
      />
      <label for={sldId}>1</label>
    </div>
  );
}

export default InputSlider;
