function [numout, meansout, stdevsout] = run_analyze(filename)
  data = csvread(filename);

[n1, n2] = size(data);
data_length = ceil(n1/2);
evens = 2*(1:data_length);
odds = evens - 1;
numout = data(evens,1);

loadtimes = [];
for i = 1:length(odds)
  loadtimes(i, :) = data(odds(i), 2:end);
end
computetimes = [];
for i = 1:length(evens)
  computetimes(i, :) = data(evens(i), 2:end);
end
meansout = mean(computetimes')';
stdevsout = std(computetimes')';
end
