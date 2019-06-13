

function() {
    //if (this.Pcs)
    
    emit(this.timestamp.toISOString().split('T')[0],
        { count: 1, pcsactpwr: this.Pcs.ActivePower, pvactpwr: this.Pv.EnergyTotalActivePower, status: this.Pcs.Status, accumchg: 0, accumdhg: 0 });
}