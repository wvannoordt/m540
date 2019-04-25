from OpenGL.GL import *
from OpenGL.GLUT import *
from OpenGL.GLU import *
from drawInit import *
from drawCube import *
import serial
import os
import threading

ESCAPE = '\033'
window = 0

#set escape key
def keyPressed(*args):
        if args[0] == ESCAPE:
                sys.exit()

def main():

        global window

        glutInit(sys.argv)
        glutInitDisplayMode(GLUT_RGBA | GLUT_DOUBLE | GLUT_DEPTH)
        glutInitWindowSize(1000,800)
        glutInitWindowPosition(400,400)

        window = glutCreateWindow('OpenGL Python Cube')

        glutDisplayFunc(DrawGLScene)
        glutIdleFunc(DrawGLScene)
        glutKeyboardFunc(keyPressed)
        InitGL(640, 480)
        glutMainLoop()

if __name__ == "__main__":
        main()


