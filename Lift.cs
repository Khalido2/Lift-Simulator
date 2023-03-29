using System.Collections;
using System.Linq;

//Lift is an FSM of 5 states (same as described in written solution plan but minus the alarm state)
public class Lift : ILift
{
    const int RESET_FLOOR_VALUE = -1; //value used when Target DDFs value needs to be reset
    const int NO_SUCH_FLOOR = -1000; //value used when there are no EFCs/DDFs in the current direction
    const int TIME_BETWEEN_FLOORS = 10; //number of seconds it takes to move from 1 floor to the next

    int CurrentState;
    Queue<ExternalFloorCall>[] EFCs; 
    string[] People; //stores people in the lift 
    int[] DDFs; //array to store desired destination floors
    int NumFloors; //total number of floors in the building
    int Capacity;
    int PeopleCount; //number of currently in the lift
    int CurrentFloor;
    int CurrentTime;
    int TransitTime; //time remaining till lift arrives at target floor
    int TargetDDF; //the next desired destination floor that the lift will go to 
    int TargetFloor; //floor the lift will arrive at next (not necessarily the target DDF could be an EFC on the way)
    int Direction; //lift direction, 1 for UP and -1 for DOWN
    int CallCount; //number of active EFCs that need to be responded to
    int TotalResponseTime; //total time taken for lift to respond to each EFC
    string FloorRecord; //the recorded log of information for floor-to-floor activities
    int OldDirection;

    //Initalise variables needed for set up
    public Lift(int Capacity, int StartingFloor, int NumFloors,int Time) 
    {
        this.Capacity = Capacity;
        this.CurrentFloor = StartingFloor;
        this.CurrentTime = Time;
        this.NumFloors = NumFloors;
        People = new string[NumFloors];
        DDFs = new int[NumFloors];
        Direction = 1;
        OldDirection = Direction;
        FloorRecord = "";
        EFCs = new Queue<ExternalFloorCall>[NumFloors];

        for(int i = 0; i < NumFloors; i++){
            EFCs[i] = new Queue<ExternalFloorCall>();
        }

        State0(); //start in state 0
    }

    //Handles arrival of lift at each floor as well as incrementing of lift clock and reduction of transit time
    public void Update()
    {
        CurrentTime++;

        if(TransitTime>0)
            TransitTime--;

        if(TransitTime==0 && CurrentFloor!=TargetFloor){ //if we have just arrived at a new floor
            CurrentFloor+=Direction;
            if(CurrentFloor==TargetDDF) //if this floor was a DDF
            {
                DDFs[CurrentFloor]=0; //reset marker to remove it from queue
                int LeavingCount = People[CurrentFloor].Count(t => t == ' '); //count how many ppl got off at this floor
                People[CurrentFloor] = "";
                PeopleCount-=LeavingCount;
                TargetDDF=RESET_FLOOR_VALUE;
            } 
            
            if(EFCs[CurrentFloor].Count != 0 && !AtCapacity()) //if an EFC was made on this floor handle it
                HandleEFC();

            //switch directions if at top-most or bottom-most floor or last DDF in this direction has been reached
            if(CurrentFloor == 0 || CurrentFloor == NumFloors-1 || GetNearestDDF()==NO_SUCH_FLOOR) 
                ChangeDirection();

            RunState(); //recalculate and run state

            WriteFloorRecord();
        }else if(TransitTime%TIME_BETWEEN_FLOORS==0&&CurrentFloor!=TargetFloor){
            CurrentFloor+=Direction; //increase floor value as the lift moves in transit to the target floor
        }
    }

    //Called when lift has arrived on a floor in which an EFC was made
    void HandleEFC(){
        int Counter = 0;
        //add as many EFCs as you can before capacity reached
        while(EFCs[CurrentFloor].Count > 0 && !AtCapacity()){
            ExternalFloorCall EFC = EFCs[CurrentFloor].Dequeue();
            TotalResponseTime+=CurrentTime-EFC.CallTime; //add response time to total
            People[EFC.DesiredFloor]+=" "+EFC.PersonID;
            DDFs[EFC.DesiredFloor] = 1; //set DDF
            PeopleCount++;
            CallCount--;
            Counter++;
        }

        TargetDDF=RESET_FLOOR_VALUE; //reset target DDF incase someone new onboard has a closer one
    }

    //Make an EFC to the lift
    public void MakeEFC(ExternalFloorCall EFC)
    {
        EFCs[EFC.CallFloor].Enqueue(EFC);
        CallCount++;
        RunState(); //recalculate state in case it has changed
    }

    //Calculate current state of lift according to spec and run it
    void RunState(){
        if(IsEmpty())
        {
            if(CallCount>0)
                State1();
            else
                State0();

        }else{
            if(AtCapacity()){
                State4();
            }else{
                if(CallCount>0)
                    State3();
                else
                    State2();
            }
        }
    }

    //Lift is empty and there are no EFCs
    void State0(){
        //in the morning lift should be at floor 0
        //then from midday be at the middle floor (floor 5 in this scenario)
        //time frame of prototype isn't a whole day so just go to floor 5
        CurrentState = 0;
        SetTargetFloor(NumFloors/2);
    }

    //Lift is empty but there are some EFCs
    //lift can go anywhere so goes to nearest call
    void State1(){
        CurrentState = 1;
        int NearestUp = GetNearestEFC(1);
        int NearestDown = GetNearestEFC(-1);

        if(Math.Abs(CurrentFloor-NearestDown) < Math.Abs(CurrentFloor-NearestUp)){
            Direction = -1;
            SetTargetFloor(NearestDown);
        }else{
            Direction = 1;
            SetTargetFloor(NearestUp);
        }
    }

    //Lift is occupied there are DDFs but no EFCs
    //Lift goes to DDF
    void State2(){
        CurrentState = 2;
        if(TargetDDF == RESET_FLOOR_VALUE) //set if target not set 
            TargetDDF = GetNearestDDF(); 
        
        SetTargetFloor(TargetDDF);
    }

    //Lift is occupied there are DDFs and EFCs
    //lift aims at closest DDF in current direction but will stop at closest EFC if it is on the way
    void State3(){
        CurrentState = 3;
        int nearestEFC = GetNearestEFC(Direction);

        if(TargetDDF == RESET_FLOOR_VALUE) //calculate target DDF if not set
            TargetDDF = GetNearestDDF();

        if(Math.Abs(nearestEFC-CurrentFloor) < Math.Abs(TargetDDF-CurrentFloor)){
            SetTargetFloor(nearestEFC);
        }else{
            SetTargetFloor(TargetDDF);
        }

    }

    //At max capacity
    //Lift ignores any EFCs and handles DDFs
    void State4(){
        CurrentState = 4;

        if(TargetDDF == RESET_FLOOR_VALUE) //calculate target DDF if not set
            TargetDDF = GetNearestDDF();

        SetTargetFloor(TargetDDF);
    }


    //Gets nearest EFC to current floor in a given direction
    int GetNearestEFC(int Dir){
        if(CallCount == 0) //no calls at all
            return NO_SUCH_FLOOR; 

        //EFC is only on the same floor as the lift if its not in transit i.e. transit time == 0 so only count it when this is the case
        int startVal = (TransitTime==0?CurrentFloor:CurrentFloor+Dir);  

        for(int i = startVal; i < NumFloors && i >= 0; i+=Dir){
            if(EFCs[i].Count>0)
                return i;
        }

        return NO_SUCH_FLOOR; //no calls in given direction 
    }

    //Gets nearest DDF to current floor in a direction
    int GetNearestDDF(){
        for(int i = CurrentFloor; i < NumFloors && i >= 0; i+=Direction){
            if(DDFs[i]==1)
                return i;
        }

        return NO_SUCH_FLOOR; //there are no DDFs in current direction 
    }
    
    void SetTargetFloor(int Floor){
        if(Floor >= 0 && Floor < NumFloors && Floor != TargetFloor){
            TargetFloor = Floor;

            int RemainderTransitTime =TransitTime%TIME_BETWEEN_FLOORS; //calculate how far lift was from next floor in old direction

            if(RemainderTransitTime>0)
                RemainderTransitTime = TIME_BETWEEN_FLOORS-RemainderTransitTime;
            
            if(OldDirection==Direction){//if moving in same dir as before, reduce transit time by amount of time already spent travelling there
             RemainderTransitTime*=-1;
            }

            TransitTime = (Math.Abs(TargetFloor-CurrentFloor)*TIME_BETWEEN_FLOORS) + RemainderTransitTime; //add or remove that remainder time from new transit time
            OldDirection = Direction;
        }
    }

    //Writes new floor record for most recent floor arrival and adds it to log
    //Although lift processes floor numbers as 0-9, the log displays floor numbers as 1-10
    void WriteFloorRecord(){
        string PeopleOnLift = "";
        string LiftRouteNQueue = GetLiftRouteNQueue();

        //get IDs of people in the lift
        for(int i = 0; i < NumFloors; i++){
            PeopleOnLift += People[i];
        }

        FloorRecord+=$"\n{CurrentTime}, {PeopleOnLift.Trim()}, {CurrentState}, {CurrentFloor+1}, {LiftRouteNQueue}"; 
    }

    //Gets the current route the lift will take through the floors followed by the lift call queue
    //These are comma separated
    string GetLiftRouteNQueue(){
        string LiftRoute = "";
        string LiftCallQ = "";
        string floor;

        //Get EFCs and DDFs in current dir
        for(int i = CurrentFloor+Direction; i < NumFloors && i >= 0; i+=Direction){
            floor = (i+1) + " ";
            if(DDFs[i]==1)
                LiftRoute+= floor;

            if(EFCs[i].Count>0){
                if(CurrentState != 4 && DDFs[i] != 1) //as EFCs are ignored in state 4
                    LiftRoute+=floor;

                LiftCallQ+=floor;
            }
        }

        //Get EFCs and DDFs in opposite dir
        for(int i = CurrentFloor-Direction; i < NumFloors && i >= 0; i-=Direction){
            floor = (i+1) + " ";
            if(DDFs[i]==1)
                LiftRoute+= floor;

            if(EFCs[i].Count>0){
                if(CurrentState != 4 && DDFs[i] != 1) //as EFCs are ignored in state 4
                    LiftRoute+=floor;

                LiftCallQ+=floor;
            }
        }

        return LiftRoute.Trim()+", "+LiftCallQ.Trim();
    }

    void ChangeDirection(){
        Direction *= -1;
    }

    bool AtCapacity(){
        return(PeopleCount==Capacity?true:false);
    }

    bool IsEmpty(){
        return (PeopleCount==0?true:false);
    }

    public bool NoActivity(){
        return (IsEmpty()&&CallCount==0);
    }

    public string GetFloorRecord()
    {
        return FloorRecord;
    }

    public int GetResponseTime()
    {
        return TotalResponseTime;
    }

}