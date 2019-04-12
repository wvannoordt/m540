clear
clc
close all

[counts8 means8 stdevs8] = run_analyze('./output8.csv');%"8" cores
[counts4 means4 stdevs4] = run_analyze('./output4.csv');% 4  cores
[counts1 means1 stdevs1] = run_analyze('./output1.csv');% 1  cores

scatter(log(counts8), means8);
hold on
scatter(log(counts4), means4);
hold on
scatter(log(counts1), means1);
title('Scaling of Classification');
xlabel('Facet Count (log)');
ylabel('Compute Time (ms)');
legend('8 Processes', '4 Processes', '1 Processe');
