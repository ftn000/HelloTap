mergeInto(LibraryManager.library, {
    YandexSDK_Init: function() {
        if (typeof window.InitYandexSDK === 'function') {
            window.InitYandexSDK();
        } else {
            console.log('[YandexSDK JS] Init called. Checking YaGames global...');
            if (typeof YaGames !== 'undefined') {
                YaGames.init().then(function(ysdk) {
                    window.ysdk = ysdk;
                    console.log('[YandexSDK JS] YaGames initialized successfully.');
                    if (window.unityInstance) {
                        window.unityInstance.SendMessage('YandexSDKBridge', 'OnSDKInitializedCallback', 'true');
                    }
                }).catch(function(err) {
                    console.error('[YandexSDK JS] YaGames initialization error:', err);
                    if (window.unityInstance) {
                        window.unityInstance.SendMessage('YandexSDKBridge', 'OnSDKInitializedCallback', 'false');
                    }
                });
            } else {
                console.warn('[YandexSDK JS] YaGames is not defined (running standalone or offline).');
                if (window.unityInstance) {
                    window.unityInstance.SendMessage('YandexSDKBridge', 'OnSDKInitializedCallback', 'false');
                }
            }
        }
    },

    YandexSDK_ShowFullscreenAd: function() {
        if (typeof window.ysdk !== 'undefined' && window.ysdk.adv) {
            window.ysdk.adv.showFullscreenAdv({
                callbacks: {
                    onOpen: function() {
                        if (window.unityInstance) {
                            window.unityInstance.SendMessage('YandexSDKBridge', 'OnFullscreenAdOpened');
                        }
                    },
                    onClose: function(wasShown) {
                        if (window.unityInstance) {
                            window.unityInstance.SendMessage('YandexSDKBridge', 'OnFullscreenAdClosed', wasShown ? 'true' : 'false');
                        }
                    },
                    onError: function(error) {
                        console.warn('[YandexSDK JS] Fullscreen ad error:', error);
                        if (window.unityInstance) {
                            window.unityInstance.SendMessage('YandexSDKBridge', 'OnFullscreenAdError', error ? String(error) : 'error');
                        }
                    }
                }
            });
        } else {
            console.log('[YandexSDK JS Mock] ShowFullscreenAd called (fallback).');
            if (window.unityInstance) {
                window.unityInstance.SendMessage('YandexSDKBridge', 'OnFullscreenAdClosed', 'true');
            }
        }
    },

    YandexSDK_ShowRewardedAd: function(placementIdPtr) {
        var placementId = UTF8ToString(placementIdPtr) || 'default';
        if (typeof window.ysdk !== 'undefined' && window.ysdk.adv) {
            window.ysdk.adv.showRewardedVideo({
                callbacks: {
                    onOpen: function() {
                        if (window.unityInstance) {
                            window.unityInstance.SendMessage('YandexSDKBridge', 'OnRewardedAdOpened', placementId);
                        }
                    },
                    onRewarded: function() {
                        if (window.unityInstance) {
                            window.unityInstance.SendMessage('YandexSDKBridge', 'OnRewardedAdRewarded', placementId);
                        }
                    },
                    onClose: function() {
                        if (window.unityInstance) {
                            window.unityInstance.SendMessage('YandexSDKBridge', 'OnRewardedAdClosed', placementId);
                        }
                    },
                    onError: function(error) {
                        console.warn('[YandexSDK JS] Rewarded ad error:', error);
                        if (window.unityInstance) {
                            window.unityInstance.SendMessage('YandexSDKBridge', 'OnRewardedAdError', placementId + '|' + (error ? String(error) : 'error'));
                        }
                    }
                }
            });
        } else {
            console.log('[YandexSDK JS Mock] ShowRewardedAd called for placement: ' + placementId + ' (fallback rewarding).');
            if (window.unityInstance) {
                window.unityInstance.SendMessage('YandexSDKBridge', 'OnRewardedAdRewarded', placementId);
                window.unityInstance.SendMessage('YandexSDKBridge', 'OnRewardedAdClosed', placementId);
            }
        }
    },

    YandexSDK_SaveData: function(jsonDataPtr) {
        var jsonData = UTF8ToString(jsonDataPtr);
        if (typeof window.ysdk !== 'undefined' && window.ysdk.getPlayer) {
            window.ysdk.getPlayer().then(function(player) {
                try {
                    var parsed = JSON.parse(jsonData);
                    player.setData({ gameSave: JSON.stringify(parsed) }, true).then(function() {
                        console.log('[YandexSDK JS] Cloud data saved successfully.');
                        if (window.unityInstance) {
                            window.unityInstance.SendMessage('YandexSDKBridge', 'OnSaveCloudDataSuccess');
                        }
                    }).catch(function(e) {
                        console.error('[YandexSDK JS] Cloud data save failed:', e);
                        if (window.unityInstance) {
                            window.unityInstance.SendMessage('YandexSDKBridge', 'OnSaveCloudDataError', String(e));
                        }
                    });
                } catch (jsonErr) {
                    console.error('[YandexSDK JS] JSON parse error during save:', jsonErr);
                }
            }).catch(function(err) {
                console.warn('[YandexSDK JS] getPlayer error during save:', err);
                if (window.unityInstance) {
                    window.unityInstance.SendMessage('YandexSDKBridge', 'OnSaveCloudDataError', String(err));
                }
            });
        } else {
            console.log('[YandexSDK JS Mock] SaveData called (fallback).');
            if (window.unityInstance) {
                window.unityInstance.SendMessage('YandexSDKBridge', 'OnSaveCloudDataSuccess');
            }
        }
    },

    YandexSDK_LoadData: function() {
        if (typeof window.ysdk !== 'undefined' && window.ysdk.getPlayer) {
            window.ysdk.getPlayer().then(function(player) {
                player.getData(['gameSave']).then(function(data) {
                    var saveString = (data && data.gameSave) ? data.gameSave : '';
                    if (window.unityInstance) {
                        window.unityInstance.SendMessage('YandexSDKBridge', 'OnLoadCloudDataSuccess', saveString);
                    }
                }).catch(function(e) {
                    console.error('[YandexSDK JS] Load cloud data failed:', e);
                    if (window.unityInstance) {
                        window.unityInstance.SendMessage('YandexSDKBridge', 'OnLoadCloudDataError', String(e));
                    }
                });
            }).catch(function(err) {
                console.warn('[YandexSDK JS] getPlayer error during load:', err);
                if (window.unityInstance) {
                    window.unityInstance.SendMessage('YandexSDKBridge', 'OnLoadCloudDataError', String(err));
                }
            });
        } else {
            console.log('[YandexSDK JS Mock] LoadData called (fallback).');
            if (window.unityInstance) {
                window.unityInstance.SendMessage('YandexSDKBridge', 'OnLoadCloudDataSuccess', '');
            }
        }
    },

    YandexSDK_SetLeaderboardScore: function(lbNamePtr, score) {
        var lbName = UTF8ToString(lbNamePtr) || 'Highscore';
        if (typeof window.ysdk !== 'undefined' && window.ysdk.getLeaderboards) {
            window.ysdk.getLeaderboards().then(function(lb) {
                lb.setLeaderboardScore(lbName, score).then(function() {
                    console.log('[YandexSDK JS] Leaderboard score set: ' + score);
                }).catch(function(e) {
                    console.warn('[YandexSDK JS] Leaderboard error:', e);
                });
            }).catch(function(e) {
                console.warn('[YandexSDK JS] getLeaderboards error:', e);
            });
        } else {
            console.log('[YandexSDK JS Mock] SetLeaderboardScore: ' + lbName + ' = ' + score);
        }
    }
});
