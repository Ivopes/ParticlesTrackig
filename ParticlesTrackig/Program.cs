using ParticlesTrackig;
Directory.CreateDirectory("../../../output");
var tracker = new CellTracker("../../../unet-pred-binarize", "../../../output");
//var tracker = new CellTracker("../../../Test");
