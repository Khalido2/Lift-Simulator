public class ExternalFloorCall{

    public int CallTime{get;} //time external floor call is made
    public string PersonID{get;}
    public int CallFloor{get;} //floor that the call was made on
    public int DesiredFloor{get;}

    public ExternalFloorCall(int time, int callFloor, int desiredFloor, string personID){
        CallTime = time;
        this.CallFloor = callFloor;
        this.DesiredFloor = desiredFloor;
        this.PersonID = personID.Trim(); //remove any white spaces
    }

}