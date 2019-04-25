from OpenGL.GL import *
from OpenGL.GLUT import *
from OpenGL.GLU import *
import serial
import os
import threading

#rotation
xAxis = 0.0
yAxis = 0.0
zAxis = 0.0

direction = -1
 
def DrawGLScene():
        global xAxis, yAxis, zAxis
        global direction
 
        glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT)
 
        glLoadIdentity() #replaces input with identity matrix
        glTranslatef(1.0,0.0,-6.0)

	#glRotatef(angle, x, y, z) 

        glRotatef(xAxis,1.0,0.0,0.0)
        glRotatef(yAxis,0.0,1.0,0.0)
        glRotatef(zAxis,0.0,0.0,1.0)
 
        #draw shapes. good shapes are: (QUADS, LINES, POINTS)
        glBegin(GL_LINES)
 
	#glColor3f(red, green, blue)
	#glVertex3f(x, y, z)

	glColor3f(1.0,0.0,0.0)
	glVertex3f( 1.0, 1.0,-1.0)
        glVertex3f(-1.0, 1.0,-1.0)
        glVertex3f(-1.0, 1.0, 1.0)
        glVertex3f( 1.0, 1.0, 1.0) 
 
	glColor3f(1.0,0.0,0.0)
        glVertex3f( 1.0,-1.0, 1.0)
        glVertex3f(-1.0,-1.0, 1.0)
        glVertex3f(-1.0,-1.0,-1.0)
        glVertex3f( 1.0,-1.0,-1.0) 
 
	glColor3f(1.0,1.0,0.0)
        glVertex3f( 1.0, 1.0, 1.0)
        glVertex3f(-1.0, 1.0, 1.0)
        glVertex3f(-1.0,-1.0, 1.0)
        glVertex3f( 1.0,-1.0, 1.0)
 
	glColor3f(1.0,1.0,0.0)
        glVertex3f( 1.0,-1.0,-1.0)
        glVertex3f(-1.0,-1.0,-1.0)
        glVertex3f(-1.0, 1.0,-1.0)
        glVertex3f( 1.0, 1.0,-1.0)
 
	glColor3f(0.0,1.0,1.0)
        glVertex3f(-1.0, 1.0, 1.0) 
        glVertex3f(-1.0, 1.0,-1.0)
        glVertex3f(-1.0,-1.0,-1.0) 
        glVertex3f(-1.0,-1.0, 1.0) 
 
	glColor3f(0.0,1.0,1.0)
        glVertex3f( 1.0, 1.0,-1.0) 
        glVertex3f( 1.0, 1.0, 1.0)
        glVertex3f( 1.0,-1.0, 1.0)
        glVertex3f( 1.0,-1.0,-1.0)

        glEnd()

	#comment the following three lines to stop rotation
	xAxis = xAxis - 1.0
	yAxis = yAxis + 2.0
 	zAxis = yAxis + 1.5
	
        glutSwapBuffers()
 
 
