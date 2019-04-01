function [complexForce] = DampForce(root, rootVelocity, position, positionVelocity,settings)
%This function returns the damping force that is a result of the damped extension of the
%spring. It is proportional to the rate of change in (arc) length of the spring. It is
%invariant to the velocity of the node (to some degree).
bs = settings.springDampingCoefficient;
length1 = abs(root-position);
length2 = abs((root+settings.timeStep*rootVelocity)-position+settings.timeStep*positionVelocity);
dldt = (length2-length1)/settings.timeStep;
complexForce = -bs*(dldt*(root-position)/length1);
end