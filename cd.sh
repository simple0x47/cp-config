#!/bin/bash

HOST_SSH_PRIVATE_KEY=$(bws secret get "7ad8e4fa-477a-425b-8377-b09e00885ab3" --access-token "$SECRETS_MANAGER_ACCESS_TOKEN" | jq -r '.value')

mkdir ~/.ssh
echo "$HOST_SSH_PRIVATE_KEY" > ~/.ssh/host_ssh_private_key
chmod 600 ~/.ssh/host_ssh_private_key

eval `ssh-agent`
ssh-add ~/.ssh/*

ssh -o StrictHostKeyChecking=accept-new $SSH_CONNECTION  'cd /home/gabriel/dev/cp-deployment/cp-config && git pull && microk8s kubectl rollout restart -f deployment.yaml'