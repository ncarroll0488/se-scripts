int main(string collectorID) {

    //Set this to true if you have an LCD panel for status updates. It needs to be called '<collectorID>.Status Panel'
    var statusTracker = false;
    var statusString = "ID: ";

    //If we feed the script an argument, we add a dot to the end of that argument. This is used later as a prefix when referencing blocks by name
    if (!String.IsNullOrEmpty(collectorID)) {
	statusString = statusString + collectorID;
	collectorID = collectorID + ".";
    } else {
	statusString = statusString + "None";
	collectorID = "";  //Is this necessary?
    }
    statusString = statusString + "\n";
    //If collectorID was specified earlier, then these will be 'foo.Negative Detector', etc.... Otherwise, they'll just be 'Negative Detector'
    //Grab our solar panels
    var positiveDetector = GridTerminalSystem.GetBlockWithName(collectorID + "Positive Detector") as IMySolarPanel;
    var negativeDetector = GridTerminalSystem.GetBlockWithName(collectorID + "Negative Detector") as IMySolarPanel;

    //Grab our rotors - unless you've set statusTracker, only pitch is used.
    var yawControl = GridTerminalSystem.GetBlockWithName(collectorID + "Yaw Rotor") as IMyMotorAdvancedStator;
    var rollControl = GridTerminalSystem.GetBlockWithName(collectorID + "Roll Rotor") as IMyMotorAdvancedStator;
    var pitchControl = GridTerminalSystem.GetBlockWithName(collectorID + "Pitch Rotor") as IMyMotorAdvancedStator;
    
    //Grab our statusPanel if statusTracker is set
    if (statusTracker) {
        var statusPanel = GridTerminalSystem.GetBlockWithName(collectorID + "Status Panel") as IMyTextPanel;
    }

    //Get current rotor positions
    var currentYaw = numbersOnly(yawControl.DetailedInfo);
    var currentRoll = numbersOnly(rollControl.DetailedInfo);
    var currentPitch = numbersOnly(pitchControl.DetailedInfo);

    statusString = statusString  + "yaw: " + currentYaw + " | roll: " + currentRoll + " | pitch: " + currentPitch + "\n";

    //Untested - source code indicates this might be possible
    var positiveOutput = positiveDetector.MaxOutput;
    var negativeOutput = negativeDetector.MaxOutput;
    statusString = statusString + "p: " + positiveOutput + "kW | n: " + negativeOutput + "kW\n";

    //Untested - source code indicates this might be possible
    //Get current velocity from the pitch rotor, and determine the positive and negative vals from it.
    var targetPitchVelocity = 0;
    var positivePitchVelocity = System.Math.Abs(pitchControl.TargetVelocityRPM);
    var negativePitchVelocity = (0 - positivePitchVelocity);

    //If at least one panel is on and non-equal output
    if (((postiveOutput + negativeOutput) > 0) && ((positiveOutput - negativeOutput) <> 0)) {
	if (positiveOutput > negativeOutput) {
	    targetPitchVelocity = positivePitchVelocity;
	} else {
	    //Set negative velocity if the negative panel has more output
	    targetPitchVelocity = negativePitchVelocity;
	}
	//Turn the pitch rotor on to start tracking
	pitchControl.TargetVelocityRPM = negativePitchVelocity;
	pitchControl.applyAction("OnOff_On");
    } else {
	//Zero or equal output. Turn the pitch rotor off
	pitchControl.applyAction("OnOff_Off");
    }
    statusString = statusString + "v: " System.Convert.ToString(targetPitchVelocity);
    if (statusTracker) {
	statusPanel.writePublicText(statusString,false);
    }
}

string numbersOnly(string blob) {
    return System.Text.RegularExpressions.Regex.Replace("[^-\.0-9]",blob);
}
