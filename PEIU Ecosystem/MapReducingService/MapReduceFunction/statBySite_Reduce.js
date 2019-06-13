function(key, values) {
    var result = {
        accum_energykW: 0, timestamp: new Date().toLocaleString(), accum_chargingkW: 0, accum_dischargingkW: 0 };

    values.forEach(function (value) {        
        if ((value.status & 0x04) == 0x04 && isNaN(value.pcsactpwr) == false) {
            result.accum_chargingkW += value.pcsactpwr;
        }
        if ((value.status & 0x05) == 0x05 && isNaN(value.pcsactpwr) == false) {
            result.accum_dischargingkW += value.pcsactpwr;
        }

        if (isNaN(value.pvactpwr) == false)
            result.accum_energykW += value.pvactpwr;
        
    });
    result.accum_dischargingkW = result.accum_dischargingkW / 3600;
    result.accum_chargingkW = result.accum_chargingkW / 3600;
    //if (result.accum_energyMW != 0)
    result.accum_energykW = result.accum_energykW / 3600;

    return result;
}