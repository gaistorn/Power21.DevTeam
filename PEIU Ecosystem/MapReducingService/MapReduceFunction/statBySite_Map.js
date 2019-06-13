

function() {
    //if (this.Pcs)
    
    emit({ siteid: this.sSiteId, timestamp: this.timestamp.toISOString().split('T')[0] }, { pcsactpwr: this.Pcs.ActivePower, pvactpwr: this.Pv.TotalActivePower, status: this.Pcs.Status, accumchg: 0, accumdhg: 0 });
}