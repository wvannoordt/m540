function [entryOut] = BoundaryAccess(matrix, row, column, settings, timeindex)
%This function acts as an extension of a matrix array access. Since boundary positions and
%velocities (see vBoundaryAccess) are pre-specified, these values must be accessed when
%index values exceed domain limits.

%This line needs no comment
[Ny, Nx] = size(matrix);

%Detect if on any boundary at all
boundary = (row == 0 || column == 0 || row == (Ny+1) || column == (Nx+1));
if ~boundary
    entryOut = matrix(row, column);
	
	
else

	%If access indices lie on a boundary, return the value specified in settings.
	%This function does not account for access on "corner nodes" (i.e. (0,0)).
	
	%I don't know if this line is necessary but I am too lazy to check because MATLAB takes
	%forever to start up.
    entryOut = (column / (1 + Nx)) + 1i*((Ny + 1 - row) / (1 + Ny));
    if row == 0 %up
        entryOut = entryOut + settings.wallDisplacementProfile(1, timeindex);
    end
    if column == 0 %left
        entryOut = entryOut + settings.wallDisplacementProfile(3, timeindex);
    end
    if row == (Ny + 1) %down
        entryOut = entryOut + settings.wallDisplacementProfile(2, timeindex);
    end
    if column == (Nx + 1) %right
        entryOut = entryOut + settings.wallDisplacementProfile(4, timeindex);
    end
	
end
end

