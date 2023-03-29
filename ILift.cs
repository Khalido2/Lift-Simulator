
//Interface that World object uses to interact with the lift algorithm
public interface ILift {

    //Performs one cycle of lift operation i.e processes that would be performed by the lift each sec
    public void Update();

    //Makes an external floor call to the lift
    public void MakeEFC(ExternalFloorCall EFC);

    //Returns floor-to-floor record for output CSV file
    public string GetFloorRecord();

    //Returns total amount amount of time taken to respond to each efc
    public int GetResponseTime();

    //Indicates when the lift has no ETCs nor is anyone in the lift
    public bool NoActivity();
}