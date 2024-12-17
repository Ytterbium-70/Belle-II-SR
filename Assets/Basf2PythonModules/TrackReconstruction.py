import os
import math
import pandas as pd

import basf2 as b2
import generators as ge
import simulation as si
import reconstruction as re
from ROOT import Belle2
import modularAnalysis as ma

inputFile = input("Enter Input File Name: ") or "/project/agkuhr/users/thomas.lueck/testSampleB0B0genericMC/mdst_000210_prod00016816_task10020000211.root"
fileNumber = int(input("Please the first file number (default = 1): ") or 1)

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

        with open(f"track{TurnThreeDigit(fileNumber + ReconstructTracks.fileCount)}.csv", "w") as file:
            file.write("TrackID,ParticleName,PosX,PosY,PosZ,Probability,TrackDistance" + "\n")
        
        particles = Belle2.PyStoreArray("Particles")
        
        for i in range(len(particles)):
            printProgressBar(i, len(particles), prefix=f"track{TurnThreeDigit(fileNumber + ReconstructTracks.fileCount)}.csv ")

            lh = particles[i].getPIDLikelihood()
            #if lh := particles[i].getPIDLikelihood():
            if lh:
                ptype = lh.getMostLikely()
                if abs(ptype.getPDGCode()) != abs(particles[i].getPDGCode()):
                    continue

                track = particles[i].getTrack()
                helixTrack = track.getTrackFitResultWithClosestMass(ptype).getHelix()
                patName = ptype.getParticlePDG().GetName() # oder PdgCode() statt GetName() falls das besser passt
                trackId = particles[i].getMdstArrayIndex()

                ReconstructTracks.trackId += 1
                
                #extract the coordinates and write them into a csv file
                for dis in range(1000):
                    pos = helixTrack.getPositionAtArcLength2D(dis)

                    radius = math.sqrt( math.pow(pos.X(), 2) + math.pow(pos.Y(), 2) )
                    
                    if patName == "mu-" or patName == "mu+":#muons pass through the detector, other particles are stopped at the ECL
                        if radius < 350 and -300 < pos.Z() < 300:
                            with open(f"track{TurnThreeDigit(fileNumber + ReconstructTracks.fileCount)}.csv", "a") as file:
                                file.write(f"{ReconstructTracks.trackId},{patName},{pos.X()},{pos.Y()},{pos.Z()},{dis},{trackId}" + "\n")
                        else:
                            break
                    else:
                        if radius < 125 and -102 < pos.Z() < 196:
                            with open(f"track{TurnThreeDigit(fileNumber + ReconstructTracks.fileCount)}.csv", "a") as file:
                                file.write(f"{ReconstructTracks.trackId},{patName},{pos.X()},{pos.Y()},{pos.Z()},{dis},{trackId}" + "\n")
                        else:
                            break

main = b2.create_path()
#main.add_module('Progress')

ma.inputMdst(inputFile, main)

#re.add_reconstruction(main)

ma.fillParticleList("pi+","",writeOut=True,path=main)
ma.fillParticleList("K+","",writeOut=True,path=main)
ma.fillParticleList("mu+","",writeOut=True,path=main)

main.add_module(ReconstructTracks())

b2.process(path=main, max_event=10)
