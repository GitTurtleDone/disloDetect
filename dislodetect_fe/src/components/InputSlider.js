import { useEffect, useState } from "react";
function InputSlider(props) {
  const { iptText, iptId, sldId, iptStep, sldStep, dftVal, updateVal } = props;
  const [value, setValue] = useState(parseFloat(dftVal));
  // const [finalValue, setFinalValue] = useState(parseFloat(dftVal));
  const updateSlider = (e) => {
    setValue(e.target.value);
    // updateVal(e.target.value);
  };
  const updateInput = (e) => {
    const iptVal = e.target.value;
    if (!isNaN(iptVal) && iptVal >= 0 && iptVal <= 1) {
      setValue(Number(iptVal));
    }
    // updateVal(iptVal);
  };

  //  below use effect is for updating the slider and input in 500 ms intervals 
  //  using deboucing value as the final ones
  useEffect(()=>{
    const hdlTimeout = setTimeout(() =>{
      // setFinalValue(value);
      updateVal(value);
    }, 500);
    return () => {
      clearTimeout(hdlTimeout);
    };
  }, [value]);
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
        id={sldId}
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
