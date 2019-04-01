function [entryOut] = vBoundaryAccess(matrix, row, column, settings, timeindex)
[Ny, Nx] = size(matrix);
%For comments, see BoundaryAccess.m
boundary = (row == 0 || column == 0 || row == (Ny+1) || column == (Nx+1));
if ~boundary
    entryOut = matrix(row, column);
else
    entryOut = 0;
end
end