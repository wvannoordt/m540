"""
Plots nodes and edges frame by frame. Animate these with ImageMagick (or something similar).
"""

import matplotlib.pyplot as plt
from mpl_toolkits.mplot3d import Axes3D


# import edge position data file
fedges = open("/Users/tristanrendfrey/files/math/m540-master/format-example/edge.csv", "r")
#print(f.readlines())
edges = []
for line in fedges.readlines():
    line = line[0:13]
    lineIn = line.split(",")
    edges.append(lineIn)

#import node position data file
fnodes = open("/Users/tristanrendfrey/files/math/m540-master/format-example/node.csv", "r")
nodesComplete = []
for line in fnodes:
    line = line[0:5]
    lineIn = line.split(",")
    nodesComplete.append(lineIn)

frames = 1      #frames to iterate over (just bounds for line 30)
points = 8      #number of individual nodes present in frame
i = 0           #index for block of points

for frameIter in range(0,frames):
    
    #get list of nodes for the frame from block
    nodes = nodesComplete[i:points+i]
    
    #get node positions for frame
    xs = []
    ys = []
    zs = []
    count = 0
    for n in nodes:
        x = float(n[0])
        y = float(n[1])
        z = float(n[2])
        xs.append(x)
        ys.append(y)
        zs.append(z)
        count += 1
        if count > points:
            break
        
    #draw the frame and plot it
    fig = plt.figure()
    ax = Axes3D(fig)
    
    ax.scatter(xs, ys, zs, c='r', marker='o')
    
    filename='/Users/tristanrendfrey/files/math/frame'+str(frameIter)+'.png'
    plt.savefig(filename, dpi=144)
    plt.gca()
        
    i += points
    
#plot.show()
