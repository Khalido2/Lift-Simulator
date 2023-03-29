using System.IO;

/*This world class simulates the real world by providing a clock and initiating each external floor call 
according to that clock.*/
public class World {

        static int _time = 0;
        static int _liftCapacity = 8;
        static int _numFloors = 10;
        static int _liftStartFloor = 0; //the floor that the lift starts on
        static string _filePath = "Cloud Software Engineer Coding Exercise Data.csv";
        static string _outputFilePath = "Lift Output.csv";
        static string _outputHeadings = "Current Time, People in lift, Current lift state, Current lift floor, Current lift route, Lift call queue";

        public static void Main(string[] args){

            string[] lines;
            try {
                lines = System.IO.File.ReadLines(_filePath).ToArray();
            }catch(FileNotFoundException){
                Console.WriteLine("File Not Found");
                return;
            }

            if(lines.Count() < 2) //handles rare case where CSV file is empty (it assumed that CSV has header values in row 0)
                return;

            int lineIndex = 1; //start from line 1 
            string[] currentLine = lines[lineIndex].Split(",");
            ILift lift = new Lift(_liftCapacity, _liftStartFloor, _numFloors, _time);
            
            while (lineIndex < lines.Count() || !lift.NoActivity()) //effectively a game loop that runs the entire lift environment 
            {
                _time++;
                //if current time is a time that corresponds with a row then make external floor call (EFC) to lift
                //keep iterating through rows until condition to ensure all efc's at present time are made
                while(currentLine[3] == _time+""){ 
                    //make new EFC
                    ExternalFloorCall EFC = new ExternalFloorCall(_time, Convert.ToInt32(currentLine[1])-1, Convert.ToInt32(currentLine[2])-1, currentLine[0]);
                    lift.MakeEFC(EFC);
                    lineIndex++;
                    if(lineIndex<lines.Count())
                    {
                        currentLine = lines[lineIndex].Split(",");
                    }else{
                        break;
                    }

                }

                lift.Update(); //update lift
            }

            //write floor record to CSV
            try{
                System.IO.File.WriteAllText(_outputFilePath, _outputHeadings+lift.GetFloorRecord());
            }catch (Exception e){
                Console.WriteLine("Floor records could not be written to output: "+e.ToString());
            }

            //output response time
            Console.WriteLine($"Total Response Time: {lift.GetResponseTime()}, Average Response Time per call: {lift.GetResponseTime()/(lineIndex-1)}");
        }


}