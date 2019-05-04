"""
Plots nodes and edges frame by frame. Animate these with ImageMagick (or something similar).
"""

import csv
import numpy as np
import matplotlib.pyplot as plt
from mpl_toolkits.mplot3d import Axes3D
import vispy.scene
from vispy import app, gloo, visuals
from vispy.scene import visuals


canvas = vispy.scene.SceneCanvas(title = 'Nodes and Edges', size = (800,600), keys='interactive', show=True, bgcolor='white')
view = canvas.central_widget.add_view()

nodeRead = [] #using to read and transfer nodes into an array
with open('/Users/shuke/Documents/M540/node.csv') as csvDataFile:
    reader = csv.reader(csvDataFile,quoting=csv.QUOTE_NONNUMERIC) #Changes everything to a float
    for row in reader: #each row is a list
        nodeRead.append(row)
nodes=np.array(nodeRead) #Nodes is now our array of nodes which is nX3

numberNodes= len(nodes)

#This will save the edges into four different arrays of the node1, node2 , springConst, and broken as a bool
with open('/Users/shuke/Documents/M540/edge.csv') as csvDataFile2:
    edges = csv.reader(csvDataFile2, delimiter = ',')
    node1=[]
    node2=[]
    springConst=[]
    broken=[]

    for row in edges: #Reads a row as {node 1, node2 , spring constant, True / False for broken}
        node1Int = int(row[0])
        node2Int = int(row[1])
        springConstFloat = float(row[2])
        brokenString = float(row[3])
        node1.append(node1Int)
        node2.append(node2Int)
        springConst.append(springConstFloat)
        broken.append(brokenString)


numberConnections = len(node1) #Gets the total number of points as long as something isn't screwed up

#Setting up the edges to be plotted
pos=np.zeros((numberNodes,3),dtype = np.float32)
pos[:,0] = nodes[:,0]
pos[:,1] = nodes[:,1]
pos[:,2] = nodes[:,2]

connections = np.empty((numberConnections, 2), np.int32)
connections[:,0] = node1
connections[:,1] = node2
#Plotting the data


# create scatter object and fill in the data
scatter = visuals.Markers()
scatter.set_data(nodes, edge_color=None, face_color=(0,0 , 1, 1), size=5)

view.add(scatter)
lines=vispy.scene.visuals.Arrow(pos, connect=connections, color = (1,0,0,.5) ,arrow_size=None)
view.add(lines)
view.camera = 'turntable'  # or try 'arcball'



if __name__ == '__main__':
    import sys
    if sys.flags.interactive != 1:
        vispy.app.run()
