# Lift-Simulator
An implementation of a lift algorithm in C#

As part of an extensive interview process I was put in the role of a Chief Engineer and tasked with designing a lift system for a fictional hotel called Medicine Chest.

The specification for the task was as follows:
- There are ten floors in the block
- The people working in the offices are distributed evenly across all floors
- People start arriving for work at around 8 AM, and everyone has gone home by 6 PM
- The lift needs only one external call button on each floor, instead of the usual up/down buttons. Inside the lift
should be a set of buttons to send the lift to a desired floor, as with any other lift
- The lift should have a maximum capacity of eight people at any given time

There was also an extension task to design a lift system for a new bigger hotel called the Riz Hotel. This was the spec for the extension:
- The hotel has 30 floors, and people are no longer distributed evenly across all of them. The ground and 15th
floors are much more popular, for example.
- The hotel would like four lifts installed instead of one.

My written solution included, explanations, pseudocode and UML diagrams. This can be seen in "Lift Solution.pdf"
[Lift Solution.pdf](https://github.com/Khalido2/Lift-Simulator/files/11102817/Lift.Solution.pdf)

The code implementation follows the lift solution quite closely. It uses a game loop system as the intention is to have the final program display a graphic as the lift moves.

Explanation of code implementation:
[MedicineChest Lift Prototype Documentation.pdf](https://github.com/Khalido2/Lift-Simulator/files/11102874/MedicineChest.Lift.Prototype.Documentation.pdf)

TO UNDERSTAND FORMAT OF LIFT INPUT TAKE A LOOK AT "Lift Input.csv". It describes which people get on which floors and when and to which destinations.

#TO RUN
- To run the prototype ensure .Net 7 is installed. Then, navigate to the directory of the project folder and run the command “dotnet run”. After running the application you will find the output CSV file in the project directory under the name “Lift Output.csv”. 
- Currently the input CSV file has to be called "Lift Input.csv" a command input for this will be added later
