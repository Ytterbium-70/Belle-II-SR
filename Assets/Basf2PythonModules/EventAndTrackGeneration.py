import os
import math
import pandas as pd

import basf2 as b2
import generators as ge
import simulation as si
import reconstruction as re
from ROOT import Belle2

startFileNumber = int(input("Please set the first file number (default = 1): ") or 1)
numberOfFiles = int(input("Please enter number of files (default = 1): ") or 1)

def printProgressBar (iteration, total, prefix = 'Progress:', suffix = 'Complete', decimals = 1, length = 50, fill = '|', printEnd = "\r"):
    percent = ("{0:." + str(decimals) + "f}").format(100 * (iteration / float(total)))
    filledLength = int(length * iteration // total)
    bar = fill * filledLength + '-' * (length - filledLength)
    print(f'\r{prefix} |{bar}| {percent}% {suffix}', end = printEnd)
    # Print New Line on Complete
    if iteration == total: 
        print()

def TurnThreeDigit(number):
    n = f"{number}"

    for j in range(3 - len(n)):
        n = "0" + n
    
    return n

class ReconstructTracks (b2.Module):
    trackId = -1
    fileCount = -1

    def __init__(self):
        super().__init__()
        

    def event(self):
        ReconstructTracks.trackId = -1 #Numbering for the tracks in the output files
        ReconstructTracks.fileCount += 1

        with open(f"event{TurnThreeDigit(startFileNumber + ReconstructTracks.fileCount)}_tracks.csv", "w") as file:
            file.write("TrackID,ParticleName,PosX,PosY,PosZ,Probability,TrackDistance" + "\n")
        
        #self.tracks = Belle2.PyStoreArray("Tracks")
        particles = Belle2.PyStoreArray("Particles")
        
        for i in range(len(particles)):
            fileNumber = TurnThreeDigit(startFileNumber + ReconstructTracks.fileCount)
            printProgressBar(i, len(particles), prefix=f"event{fileNumber}_tracks.csv ")

            lh = particles[i].getPIDLikelihood()
            #if lh := particles[i].getPIDLikelihood():
            if lh:
                ptype = lh.getMostLikely()
                if abs(ptype.getPDGCode()) != abs(particles[i].getPDGCode()):
                    continue

                track = particles[i].getTrack()
                helixTrack = track.getTrackFitResultWithClosestMass(ptype).getHelix()
                patName = ptype.getParticlePDG().GetName() # oder PdgCode() statt GetName() falls das besser passt
                
                ReconstructTracks.trackId += 1
                
                #extract the coordinates and write them into a csv file
                for dis in range(1000):
                    pos = helixTrack.getPositionAtArcLength2D(dis)

                    radius = math.sqrt( math.pow(pos.X(), 2) + math.pow(pos.Y(), 2) )
                    
                    if patName == "mu-" or patName == "mu+":#muons pass through the detector, other particles are stopped at the ECL
                        if radius < 350 and -300 < pos.Z() < 300:
                            with open(f"event{fileNumber}_tracks.csv", "a") as file:
                                file.write(f"{ReconstructTracks.trackId},{patName},{pos.X()},{pos.Y()},{pos.Z()},{dis}" + "\n")
                        else:
                            break
                    else:
                        if radius < 125 and -102 < pos.Z() < 196:
                            with open(f"event{fileNumber}_tracks.csv", "a") as file:
                                file.write(f"{ReconstructTracks.trackId},{patName},{pos.X()},{pos.Y()},{pos.Z()},{dis}" + "\n")
                        else:
                            break

main = b2.create_path()
main.add_module("EventInfoSetter", evtNumList=[numberOfFiles], expList=[0])
#main.add_module('Progress')

ge.add_evtgen_generator(path=main, finalstate='mixed')
si.add_simulation(main, simulateT0jitter=False)
re.add_reconstruction(main)

main.add_module(ReconstructTracks())

#create csv file of the MC Sim
b2.write_simulation_steps()

b2.process(path=main)

#remove uncessessary columns in the csv file
columns = [
    "TrackID",
    "ParentID",
    "ParticleName",
    "Mass",
    "Charge",
    "StepNumber",
    "Status",
    "VolumeName",
    "DetectorName",
    "MaterialName",
    "IsFirstStepInVolume",
    "IsLastStepInVolume",
    "EnergyDeposit",
    "ProcessType",
    "ProcessName",
    "PrePointX",
    "PrePointY",
    "PrePointZ",
    "PrePointT",
    "PrePointPX",
    "PrePointPY",
    "PrePointPZ",
    "PrePointE",
    "PostPointX",
    "PostPointY",
    "PostPointZ",
    "PostPointT",
    "PostPointPX",
    "PostPointPY",
    "PostPointPZ",
    "PostPointE",
]

select_columns = [
    "TrackID",
    "ParentID",
    "ParticleName",
    "DetectorName",
    "PrePointX",
    "PrePointY",
    "PrePointZ",
    "PrePointT",
    "PostPointX",
    "PostPointY",
    "PostPointZ",
    "PostPointT",
]

for i in range(1, numberOfFiles + 1):
    fileNumber = TurnThreeDigit(startFileNumber + i - 1)
    printProgressBar(i, numberOfFiles, prefix=f"event{fileNumber}.csv ")

    input_file = f"event{i}.csv"
    output_file = f"event{fileNumber}.csv"

    df = pd.read_csv(input_file, names=columns)
    df[select_columns].to_csv(output_file, index=False)

    #delete original event file, as it's not needed anymore
    fPath = os.path.dirname(os.path.realpath(__name__)) + f"/event{i}.csv"
    if os.path.isfile(fPath):
        os.remove(fPath)
    else:
        # If it fails, inform the user.
        print("Error: %s file not found" % fPath)
    
    #Add electrons and positrons from the particle accelerator
    if os.path.isfile("ColliderParticles.csv"):
        with open(f"event{fileNumber}.csv", "r") as file:
            contents = file.readlines()
        
        with open("ColliderParticles.csv", "r") as file:
            line = file.readline()#skip 1st line, as it's the header

            line = file.readline()
            index = 1
            while line:
                contents.insert(index, line.rstrip()+"\n")
                index += 1
                line = file.readline()
        
        with open(f"event{fileNumber}.csv", "w") as file:
            contents = "".join(contents)
            file.write(contents)
    else:
        print("No file for the particles from the particle accelerator found")
